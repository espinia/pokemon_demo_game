using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

public enum BattleState
{
	StartBattle,
	ActionSelection, 
	MovementSelection,
	//para estar ocupado cuando el pokemon está congelado
	Busy,
	YesNoChoice,
	//para la pantalla de party
	PartySelectScreen,
	//para pantalla de selección de item
	ItemSelectScreen,
	ForgetMovement,
	RunTurn,
	FinishBattle
}

public enum BattleAction
{
	Move, SwitchPokemon, UseItem, Run /*ejecutar acción*/
}

public enum BattleType
{
	WildPokemon,
	Trainer,
	Leader
}

public class BattleManager : MonoBehaviour
{
	[SerializeField]
	BattleUnit playerUnit;

	[SerializeField]
	BattleUnit enemyUnit;

	[SerializeField]
	BattleDialogBox battleDialogBox;

	[SerializeField]
	PartyHUD partyHUD;

	[SerializeField]
	MoveSelectionUI moveSelectionUI;

	[SerializeField]
	GameObject pokeball;

	[SerializeField]
	Image playerImage;
	[SerializeField]
	Image trainerImage;

	public BattleState state;
	//se le pone el ? para que pueda ser null
	public BattleState? previousState;

	public BattleType type;

	//la acción devuelve un bool
	public event System.Action<bool> OnBattleFinish;

	private PokemonParty playerParty;
	private PokemonParty trainerParty;
	private Pokemon wildPokemon;

	bool isTrainerBattle = true;

	//para detectar lo rápido que presionan las teclas
	float timeSinceLastClick;
	[SerializeField]
	float timeBetweenClicks = 1.0f;

	//acción seleccionada
	int currentSelectedAction;
	int currentSelectedMovement;
	int currentSelectedPokemon;
	bool currentSelectedChoice=true;

	int escapeAttempts;

	MoveBase moveToLearn;

	public AudioClip attackClip, damageClip, levelUpClip, endBattleClip, pokeballClip, faintedClip;

	//para la batalla con entrenador
	PlayerController player;
	TrainerController trainer;

	//batallas pokemon salvaje
	public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
	{
		type = BattleType.WildPokemon;
		escapeAttempts = 0;
		this.playerParty = playerParty;
		this.wildPokemon = wildPokemon;

		StartCoroutine(SetupBattle());
	}

	//para batallas con entrenador
	//isLeader dejamos en false por facilidad
	public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader = false)
	{
		type = (isLeader ? BattleType.Leader : BattleType.Trainer);
		this.playerParty = playerParty;
		this.trainerParty = trainerParty;

		//obtenemos sus componentes, como son al mismo nivel se puede asi
		player = playerParty.GetComponent<PlayerController>();
		trainer = trainerParty.GetComponent<TrainerController>();

		StartCoroutine(SetupBattle());
	}

	public IEnumerator SetupBattle()
	{
		//estado inicial
		state = BattleState.StartBattle;
		playerUnit.ClearHUD();
		enemyUnit.ClearHUD();

		if (type == BattleType.WildPokemon)
		{
			//el battle unit del player instancia el pokemon
			//aqui se pasa de qué pokemon
			playerUnit.SetupPokemon(playerParty.GetFirstNonFaintedPokemon());

			//rellenamos la lista de movimientos
			battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);

			//se inicializa el pokemon salvaje recibido
			enemyUnit.SetupPokemon(wildPokemon);

			//este es un método que arrancaría la corrutina
			//battleDialogBox.SetDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido.");
			//se puede arrancar la corrutina desde otra clase
			//StartCoroutine(battleDialogBox.WriteDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido."));

			//siempre debe llevar el yield
			//una corrutina puede esperar a otra
			yield return battleDialogBox.SetDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido.");

		}
		else //vs entrenador
		{
			//se ocultan los pokemon
			playerUnit.gameObject.SetActive(false);
			enemyUnit.gameObject.SetActive(false);

			//se usa el local porque son coordenadas relativas de donde esta
			//se respalda para posicionarlos de nuevo
			Vector3 playerInitialPosition = playerImage.transform.localPosition;
			playerImage.transform.localPosition = playerInitialPosition - new Vector3(-400, 0, 0);
			playerImage.transform.DOLocalMoveX(playerInitialPosition.x, 0.5f);

			Vector3 trainerInitialPosition = trainerImage.transform.localPosition;
			trainerImage.transform.localPosition = trainerInitialPosition - new Vector3(400, 0, 0);
			trainerImage.transform.DOLocalMoveX(trainerInitialPosition.x, 0.5f);

			//mostramos ahora a los entrenadores, se puede animar
			playerImage.gameObject.SetActive(true);
			trainerImage.gameObject.SetActive(true);

			//configuramos los sprites
			playerImage.sprite = player.TrainerSprite;
			trainerImage.sprite = trainer.TrainerSprite;

			yield return battleDialogBox.SetDialog($"{trainer.TrainerName} quiere luchar");

			//enviar primer pokemon entrenador
			yield return trainerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x + 400, 0.5f).WaitForCompletion();
			trainerImage.gameObject.SetActive(false);
			trainerImage.transform.localPosition = trainerInitialPosition;

			enemyUnit.gameObject.SetActive(true);

			Pokemon enemyPokemon = trainerParty.GetFirstNonFaintedPokemon();

			enemyUnit.SetupPokemon(enemyPokemon);

			yield return battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {enemyPokemon.Base.Name}");

			//enviar primer pokemon jugador
			yield return playerImage.transform.DOLocalMoveX(playerImage.transform.localPosition.x - 400, 0.5f).WaitForCompletion();
			playerImage.gameObject.SetActive(false);
			playerImage.transform.localPosition = playerInitialPosition;

			playerUnit.gameObject.SetActive(true);

			Pokemon playerPokemon = playerParty.GetFirstNonFaintedPokemon();

			playerUnit.SetupPokemon(playerPokemon);

			//rellenamos la lista de movimientos
			battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);

			yield return battleDialogBox.SetDialog($"Ve {playerPokemon.Base.Name}");

		}

		//actualizamos la vista de las partys
		//solo se asignan las cajas de la interfaz
		partyHUD.InitPartyHUD();

		//player selecciona qué hacer
		PlayerActionSelection();
	}

	void BattleFinish(bool playerHasWon)
	{
		SoundManager.SharedInstance.PlaySound(endBattleClip);
		state = BattleState.FinishBattle;

		playerParty.Pokemons.ForEach(p => p.OnBattleFinish());

		OnBattleFinish(playerHasWon);
	}

	void PlayerActionSelection()
	{
		//estado de player debe seleccionar acción
		state = BattleState.ActionSelection;

		//mostramos texto a usuario
		StartCoroutine(battleDialogBox.SetDialog("Selecciona una acción"));

		//activamos panel de acciones y lo necesario
		battleDialogBox.ToggleDialogText(true);
		battleDialogBox.ToggleActions(true);

		battleDialogBox.ToggleMovements(false);

		//resaltamos acción actual y reseteamos siempre a que empiece igual
		//o podriamos dejar el último
		currentSelectedAction = 0;
		battleDialogBox.SelectAction(currentSelectedAction);
	}

	void PlayerMovementSelection()
	{
		//pasamos a que el usuario elija movimiento
		state = BattleState.MovementSelection;

		//quitamos los elementos no necesarios
		battleDialogBox.ToggleDialogText(false);
		battleDialogBox.ToggleActions(false);

		//aparece interfaz de movimientos
		battleDialogBox.ToggleMovements(true);

		//reseteamos el movimiento seleccionado
		currentSelectedMovement = 0;

		//actualizamos la lista en la interfaz
		battleDialogBox.SelectMovement(currentSelectedMovement,
				playerUnit.Pokemon.Moves[currentSelectedMovement]);
	}
	void OpenPartySelectionScreen()
	{
		state = BattleState.PartySelectScreen;

		partyHUD.SetPartyData(playerParty.Pokemons);
		//la mostramos, recordar que el script es una componente
		partyHUD.gameObject.SetActive(true);
		currentSelectedPokemon = 0;

		//si queremos marcar el actual, podría estar en la pokemonParty
		currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit.Pokemon);

		partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

	}

	IEnumerator YesNoChoice(Pokemon newTrainerPokemon)
	{
		state = BattleState.Busy;
		yield return battleDialogBox.SetDialog($"{trainer.TrainerName} va a sacar a {newTrainerPokemon.Base.Name} ¿Quieres cambiar tu pokemon?");
		state = BattleState.YesNoChoice;
		//se muestra la caja
		battleDialogBox.ToggleYesNoBox(true);
	}

	public void HandleUpdate()
	{
		//incrementamos tiempo del clic
		timeSinceLastClick += Time.deltaTime;
		//revisamos si ya detectar
		if ((timeSinceLastClick < timeBetweenClicks) || battleDialogBox.isWriting)
		{
			return;
		}

		//responder dependiendo de lo que hace el usuario
		if (state == BattleState.ActionSelection)
		{
			HandlePlayerActionSelection();
		}
		else if (state == BattleState.MovementSelection)
		{
			HandlePlayerMovementSelection();
		}
		else if (state == BattleState.PartySelectScreen)
		{
			HandlePlayerPartySelection();
		}
		else if (state == BattleState.YesNoChoice)
		{
			//manejamos yes no selection
			HandleYesNoChoice();
		}
		else if (state == BattleState.ForgetMovement)
		{
			//se supone que es un modelo de Delegado, controllador
			moveSelectionUI.HandleForgetMoveSelection(
				//parámetro
				(moveIndex) =>
				{
					//se procesa aquí mismo
					if (moveIndex < 0)
					{
						timeSinceLastClick = 0;
						return;
					}
					else
					{
						StartCoroutine(ForgetOldMove(moveIndex));
					}
				});
		}
	}

	IEnumerator ForgetOldMove(int moveIndex)
	{
		//si seleccionaron un movimiento
		timeSinceLastClick = 0;
		moveSelectionUI.gameObject.SetActive(false);
		if (moveIndex == PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
		{
			//no se aprende el nuevo
			yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} no ha aprendido {moveToLearn.Name}");
		}
		else
		{
			//olvidamos seleccionado aprende el nuevo
			MoveBase selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
			yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha olvidado {selectedMove.Name} y aprendió {moveToLearn.Name}");
			//se cambia en la lista
			playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
			//se destruye el otro??

		}

		//se limpia la referencia
		moveToLearn = null;

		//se va a LoseTurn para que se destrabe la corrutina
		state = BattleState.Busy;
	}

	void HandlePlayerActionSelection()
	{

		//recordar que el raw solo es 1 o -1
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			//reseteamos tiempo
			timeSinceLastClick = 0;

			/*
			if (currentSelectedAction ==0)
			{
				currentSelectedAction++;
			}
			else if(currentSelectedAction==1)
			{
				currentSelectedAction--;
			}*/
			//con el módulo del total de acciones también se puede
			currentSelectedAction = (currentSelectedAction + 2) % 4;
			//llamamos a dialog box para resaltar		
			battleDialogBox.SelectAction(currentSelectedAction);
		}
		else if (Input.GetAxisRaw("Horizontal") != 0)
		{
			//reseteamos tiempo
			timeSinceLastClick = 0;
			//es una simplificación de la otra
			currentSelectedAction = (currentSelectedAction + 1) % 2 +
									2 * Mathf.FloorToInt(currentSelectedAction / 2);

			battleDialogBox.SelectAction(currentSelectedAction);
		}

		if (Input.GetAxisRaw("Submit") != 0)
		{
			//reset del contador
			timeSinceLastClick = 0;
			if (currentSelectedAction == 0)
			{
				//cambiar de estado a movimiento de jugador
				PlayerMovementSelection();
			}
			else if (currentSelectedAction == 1)
			{
				//pokemon
				previousState = state;
				OpenPartySelectionScreen();
			}
			else if (currentSelectedAction == 2)
			{
				//TODO mochila
				StartCoroutine(RunTurns(BattleAction.UseItem));
			}
			else if (currentSelectedAction == 3)
			{
				//huida
				StartCoroutine(RunTurns(BattleAction.Run));
			}
		}
	}

	void HandlePlayerMovementSelection()
	{
		int oldSelectedMovement;
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			//reseteamos tiempo clic
			timeSinceLastClick = 0;

			//respaldamos
			oldSelectedMovement = currentSelectedMovement;

			//movimiento en vertical
			currentSelectedMovement = (currentSelectedMovement + 2) % 4;

			//si salimos nos quedamos
			if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
			{
				currentSelectedMovement = oldSelectedMovement;
			}
			//resaltamos en caja de dialogo
			battleDialogBox.SelectMovement(currentSelectedMovement,
				playerUnit.Pokemon.Moves[currentSelectedMovement]);
		}
		else if (Input.GetAxisRaw("Horizontal") != 0)
		{
			//reseteamos tiempo clic
			timeSinceLastClick = 0;

			//respaldamos
			oldSelectedMovement = currentSelectedMovement;

			//movimiento horizontal
			/* lo cambiamos a la forma condensada
			if (currentSelectedMovement <= 1)
			{
				//fila de arriba
				currentSelectedMovement = (currentSelectedMovement + 1) % 2;
			}
			else if (currentSelectedMovement >= 2)
			{
				//fila de abajo
				currentSelectedMovement = ((currentSelectedMovement + 1) % 2) + 2;
			}*/
			currentSelectedMovement = (currentSelectedMovement + 1) % 2 +
										2 * Mathf.FloorToInt(currentSelectedMovement / 2);

			//si salimos nos quedamos
			if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
			{
				currentSelectedMovement = oldSelectedMovement;
			}

			//resaltamos en caja de dialogo
			battleDialogBox.SelectMovement(currentSelectedMovement,
				playerUnit.Pokemon.Moves[currentSelectedMovement]);
		}

		//botón de submit
		if (Input.GetAxisRaw("Submit") != 0)
		{
			//reset del contador
			timeSinceLastClick = 0;

			//aplicar el ataque
			//desaparece la interfaz de los movements
			battleDialogBox.ToggleMovements(false);
			//aparece el de dialogo
			battleDialogBox.ToggleDialogText(true);

			//se manda a ejecutar indicando que player selecciona un move
			StartCoroutine(RunTurns(BattleAction.Move));

		}

		if (Input.GetAxisRaw("Cancel") != 0)
		{
			//regresamos a selección de acción
			PlayerActionSelection();
		}
	}

	void HandlePlayerPartySelection()
	{
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			//reseteamos tiempo clic
			timeSinceLastClick = 0;

			/*PRIMERA VERSION
			//respaldamos
			oldSelectedPokemon = currentSelectedPokemon;

			//movimiento en vertical
			currentSelectedPokemon = (currentSelectedPokemon + 2) % 6;

			//si salimos nos quedamos
			if (currentSelectedPokemon >= playerParty.Pokemons.Count)
			{
				currentSelectedPokemon = oldSelectedPokemon;
			}
			partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);
			*/
			currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical") * 2;
			currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
			partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);
		}
		else if (Input.GetAxisRaw("Horizontal") != 0)
		{
			//reseteamos tiempo clic
			timeSinceLastClick = 0;

			/*PRIMERA VERSION
			//respaldamos
			oldSelectedPokemon = currentSelectedPokemon;
			
			
			currentSelectedPokemon = (currentSelectedPokemon + 1) % 2 +
										2 * Mathf.FloorToInt(currentSelectedPokemon / 2);

			//si salimos nos quedamos
			if (currentSelectedPokemon >= playerParty.Pokemons.Count)
			{
				currentSelectedPokemon = oldSelectedPokemon;
			}
			partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);*/
			currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");
			currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
			partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);
		}

		//botón de submit
		if (Input.GetAxisRaw("Submit") != 0)
		{
			//reset del contador
			timeSinceLastClick = 0;

			Pokemon selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
			if (selectedPokemon.Hp <= 0)
			{
				partyHUD.SetMessage("No puedes enviar un pokemon debilitado");
				return;
			}
			//en esta comparación son objetos en memoria, por localidad
			//si nacieron del mismo new
			else if (selectedPokemon == playerUnit.Pokemon)
			{
				partyHUD.SetMessage("No puedes seleccionar el pokemon en batalla");
				return;
			}
			else
			{
				//ocultamos el party hud
				partyHUD.gameObject.SetActive(false);
				battleDialogBox.ToggleActions(false);

				if (previousState == BattleState.ActionSelection)
				{
					previousState = null;
					//nosotros queremos cambiar
					StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
				}
				else
				{
					//llegamos porque debilitaron pokemon
					//marcamos que está ocupado
					state = BattleState.Busy;
					StartCoroutine(SwitchPokemons(selectedPokemon));
				}
			}
		}

		if (Input.GetAxisRaw("Cancel") != 0)
		{
			if (playerUnit.Pokemon.Hp<=0)
			{
				//no se puede cancelar
				partyHUD.SetMessage("Tienes que seleccionar un pokemon para continuar");
				return;
			}

			//deshabilitamos la vista
			partyHUD.gameObject.SetActive(false);

			if (previousState == BattleState.YesNoChoice)
			{
				//
				//state = BattleState.YesNoChoice;				
				//pasamos al otro
				previousState = null;
				StartCoroutine(SendNextTrainerPokemonToBattle());
			}
			else
			{
				//regresamos a selección de acción
				PlayerActionSelection();
			}
		}
	}

	void HandleYesNoChoice()
	{
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			timeSinceLastClick = 0;
			currentSelectedChoice = !currentSelectedChoice;
		}

		battleDialogBox.SelectYesNoAction(currentSelectedChoice);

		if (Input.GetAxisRaw("Submit") != 0)
		{
			timeSinceLastClick = 0;

			battleDialogBox.ToggleYesNoBox(false);
			//lógica de la elección
			if (currentSelectedChoice)
			{
				//Si

				//se guarda estado previo
				previousState = BattleState.YesNoChoice;
				//mostrar pantalla de cambio de pokemon
				OpenPartySelectionScreen();
			}
			else
			{
				//se sigue adelante
				StartCoroutine(SendNextTrainerPokemonToBattle());
			}
		}

		if (Input.GetAxisRaw("Cancel") != 0)
		{
			timeSinceLastClick = 0;
			battleDialogBox.ToggleYesNoBox(false);
			StartCoroutine(SendNextTrainerPokemonToBattle());
		}
	}
	IEnumerator RunTurns(BattleAction playerAction)
	{
		//se empieza perdiendo turno porque aún no se sabe la prioridad
		state = BattleState.RunTurn;

		if (playerAction == BattleAction.Move)
		{
			//se seleccionan ambos movimientos y se guardan cuáles son
			playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentSelectedMovement];
			enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.RandomMove();

			bool playerGoesFirst = true;
			int enemyPriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
			int playerPriority = playerUnit.Pokemon.CurrentMove.Base.Priority;

			//revisamos prioridad de los movimientos
			if (enemyPriority > playerPriority)
			{
				playerGoesFirst = false;
			}
			else if(enemyPriority == playerPriority)
			{
				//se revisa velocidad del pokemon si no decide pripridad de movimientos
				playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
			}
			

			//se ponen en orden quien va primero
			BattleUnit firstUnit = (playerGoesFirst ? playerUnit : enemyUnit);
			BattleUnit secondUnit = (playerGoesFirst ? enemyUnit : playerUnit);

			Pokemon secondPokemon = secondUnit.Pokemon;

			//primera unidad que ejecuta
			yield return RunMovement(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
			//revisamos si recibe daño por estado alterado
			yield return RunAfterTurn(firstUnit);
			//hay que revisar si no se ha terminado la batalla
			if (state == BattleState.FinishBattle)
			{
				yield break;
			}

			//antes del otro turno verificamos que siga vivo el second unit
			if (secondPokemon.Hp > 0)
			{
				//segunda unidad ejecuta su movimiento
				yield return RunMovement(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
				//revisamos si recibe daño por estado alterado
				yield return RunAfterTurn(secondUnit);
				//hay que revisar si no se ha terminado la batalla
				if (state == BattleState.FinishBattle)
				{
					yield break;
				}
			}
		}
		else
		{
			if (playerAction == BattleAction.SwitchPokemon)
			{
				Pokemon selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
				state = BattleState.Busy;
				yield return SwitchPokemons(selectedPokemon);
			}
			else if (playerAction == BattleAction.UseItem)
			{
				battleDialogBox.ToggleActions(false);
				yield return ThrowPokeball();
			}
			else if (playerAction == BattleAction.Run)
			{
				yield return TryToEscapeFromBattle();
			}

			//turno del enemigo
			Move enemyMove = enemyUnit.Pokemon.RandomMove();
			//primera unidad que ejecuta
			yield return RunMovement(enemyUnit, playerUnit, enemyMove);
			//revisamos si recibe daño por estado alterado
			yield return RunAfterTurn(enemyUnit);
			//hay que revisar si no se ha terminado la batalla
			if (state == BattleState.FinishBattle)
			{
				yield break;
			}
		}

		if(state!=BattleState.FinishBattle)
		{
			//regresamos a que vuelva a seleccionar player
			PlayerActionSelection();
		}
	}


	IEnumerator RunMovement(BattleUnit attacker,BattleUnit target,Move move)
	{
		//Comprobar algún estado alterado que impide atacar
		bool canRunMovement = attacker.Pokemon.OnStartTurn();
		//mostrar mensajes
		yield return ShowStatsMessages(attacker.Pokemon);
		if (!canRunMovement)
		{
			//por si la vida bajó por un estado alterado
			yield return attacker.HUD.UpdatePokemonData();
			//rompemos corrutina
			yield break;
		}

		move.Pp--;

		//dialogo con el ataque
		//recordar que esto arranca la corrutina sin el start y la espera
		yield return battleDialogBox.SetDialog($"{attacker.Pokemon.Base.Name}" +
			$" ha usado {move.Base.Name}");

		if (MoveHits(move, attacker.Pokemon, target.Pokemon))
		{
			//ya que terminó el texto
			yield return RunMoveAnim(attacker, target);

			if (move.Base.MoveType == MoveType.Stats)
			{
				yield return RunMoveStats(attacker.Pokemon, target.Pokemon, move.Base.Effects,move.Base.Target);
			}
			else
			{
				//hacemos daño al otro pokemon
				DamageDescription damageDesc = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);

				//actualizamos interfaz
				yield return target.HUD.UpdatePokemonData();

				//detalle del resultado, esperando a que termine
				yield return ShowDamageDescription(damageDesc);
			}

			//revisar efectos secundarios
			if(move.Base.SecondaryEffects!=null && move.Base.SecondaryEffects.Count>0)
			{
				foreach(SecondaryMoveStatEffect secEffect  in move.Base.SecondaryEffects)
				{
					//el objetivo está vivo?
					if( (secEffect.Target==MoveTarget.Other && target.Pokemon.Hp>0)
						||
						(secEffect.Target == MoveTarget.Self && attacker.Pokemon.Hp > 0))
					{
						//tomamnos en cuenta probabilidad
						int rnd = Random.Range(0, 100);
						if(rnd<=secEffect.Chance)
						{
							//se aplica
							yield return RunMoveStats(attacker.Pokemon, target.Pokemon, secEffect, secEffect.Target);
						}
					}

				}
			}

			//por si está envenenado o algo lo cambiamos
			//if (damageDesc.Fainted)
			if (target.Pokemon.Hp <= 0)
			{
				yield return HandlePokemonFainted(target);
			}
		}
		else
		{
			yield return battleDialogBox.SetDialog($"El ataque de {attacker.Pokemon.Base.name} ha fallado");
		}		
	}

	IEnumerator RunAfterTurn(BattleUnit attacker)
	{
		if (state == BattleState.FinishBattle)
		{
			//si ya se finalizó la batalla ya no tiene caso aplicar estados alterados
			yield break;
		}

		//esperamos mientras state no sea loseTurn para que aparezca la otra 
		//pantalla de selección
		yield return new WaitUntil(()=>state == BattleState.RunTurn);

		//aplicación de estados alterados al atacante
		attacker.Pokemon.OnFinishTurn();
		yield return ShowStatsMessages(attacker.Pokemon);
		yield return attacker.HUD.UpdatePokemonData();

		//y su vida después del efecto
		if (attacker.Pokemon.Hp <= 0)
		{
			yield return HandlePokemonFainted(attacker);
		}

		//volvemos a esperar el estado por si llegamos a debilitado por estado alterado
		yield return new WaitUntil(() => state == BattleState.RunTurn);
	}

	bool MoveHits(Move move, Pokemon attacker, Pokemon target)
	{
		//estos movimientos nunca fallan
		if (move.Base.AlwaysHit)
		{
			return true;
		}

		float rnd = Random.Range(0, 100);
		float moveAccuracy = move.Base.Accuracy;

		float accuracy = attacker.StatsBoosted[Stat.Accuracy];
		float evasion = target.StatsBoosted[Stat.Evasion];

		float multiplierAcc= 1.0f + Mathf.Abs(accuracy) / 3.0f;
		float multiplierEvs= 1.0f + Mathf.Abs(evasion) / 3.0f;

		//tasa de acierto
		if(accuracy>0)
		{
			//aumenta punteria sube efectividad
			moveAccuracy *= multiplierAcc;
		}
		else
		{
			//baja punteria baja efectividad
			moveAccuracy /= multiplierAcc;
		}

		//tasa de evasion
		if (evasion > 0)
		{
			//aumenta evasion baja efectividad
			moveAccuracy /= multiplierEvs;
		}
		else
		{
			//baja evasion sube efectividad
			moveAccuracy *= multiplierEvs;
		}

		//entra en el porcentaje de probabilidad
		return rnd <= moveAccuracy;
	}

	IEnumerator RunMoveAnim(BattleUnit attacker, BattleUnit target)
	{
		attacker.PlayAttackAnimation();

		SoundManager.SharedInstance.PlaySound(attackClip);

		//esperamos
		yield return new WaitForSeconds(1.0f);

		target.PlayReceiveAttackAnimation();
		SoundManager.SharedInstance.PlaySound(damageClip);
		//esperamos
		yield return new WaitForSeconds(1.0f);
	}

	IEnumerator RunMoveStats(Pokemon attacker, Pokemon target, 
							MoveStatEffect effect, MoveTarget movetarget)
	{
		//stats boosting
		foreach (StatBoosting boost in effect.Boostings)
		{
			if (boost.target == MoveTarget.Self)
			{
				attacker.ApplyBoost(boost);
			}
			else
			{
				target.ApplyBoost(boost);
			}
		}

		//condiciones de estado
		if(effect.Status != StatusConditionID.none)
		{
			//este es el target del movimiento completo, no del boost
			if (movetarget == MoveTarget.Other)
			{
				target.SetConditionStatus(effect.Status);
			}
			else
			{
				attacker.SetConditionStatus(effect.Status);
			}
		}
		
		//condiciones de estado volátiles
		if(effect.VolatileStatus != StatusConditionID.none)
		{
			//este es el target del movimiento completo, no del boost
			if (movetarget == MoveTarget.Other)
			{
				target.SetVolatileConditionStatus(effect.VolatileStatus);
			}
			else
			{
				attacker.SetVolatileConditionStatus(effect.VolatileStatus);
			}
		}

		yield return ShowStatsMessages(attacker);
		yield return ShowStatsMessages(target);
	}
	IEnumerator ShowStatsMessages(Pokemon pokemon)
	{
		while (pokemon.StatusChangeMessages.Count>0)
		{
			string message=pokemon.StatusChangeMessages.Dequeue();
			yield return battleDialogBox.SetDialog(message);
		}
	}

	void CheckForBattleFishish(BattleUnit faintedUnit)
	{
		Debug.Log("CheckForBattleFishish");
		if(faintedUnit.IsPlayer)
		{
			Pokemon pokemon = playerParty.GetFirstNonFaintedPokemon();
			if (pokemon != null)
			{
				//abrimos ventana selección pokemon
				OpenPartySelectionScreen();
			}
			else
			{
				//ya no hay otro, perdió el player
				BattleFinish(false);
			}
		}
		else
		{
			Debug.Log("No player");
			if (type == BattleType.WildPokemon)
			{
				Debug.Log("wild");
				//perdió el pokemon salvaje
				BattleFinish(true);
			}
			else
			{
				Debug.Log("entrenador");
				//es una batalla vs entrenador
				Pokemon nextPokemon = trainerParty.GetFirstNonFaintedPokemon();

				if (nextPokemon != null)
				{
					Debug.Log("nextPokemon dif null");					

					///tiene otro pokemon por enviar
					StartCoroutine(YesNoChoice(nextPokemon));
				}
				else
				{
					//se terminó la batalla
					BattleFinish(true);
				}
			}
		}
	}

	IEnumerator SendNextTrainerPokemonToBattle()
	{
		state = BattleState.Busy;

		//lo sacamos
		Pokemon nextPokemon = trainerParty.GetFirstNonFaintedPokemon();
		//tiene otro
		enemyUnit.SetupPokemon(nextPokemon);

		yield return battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {nextPokemon.Base.Name}");

		state = BattleState.RunTurn;
	}

	IEnumerator ShowDamageDescription(DamageDescription desc)
	{
		if(desc.Critical>1f)
		{
			yield return battleDialogBox.SetDialog("Un golpe crítico!!!");
		}
		if (desc.Type > 1f)
		{
			yield return battleDialogBox.SetDialog("!Es super efectivo!");
		}
		else if(desc.Type <1f)
		{
			yield return battleDialogBox.SetDialog("No es muy efectivo...");
		}
	}

	IEnumerator SwitchPokemons(Pokemon newPokemon)
	{
		//solo si está vivo sale este mensaje
		if (playerUnit.Pokemon.Hp > 0)
		{
			yield return battleDialogBox.SetDialog($"Vuelve {playerUnit.Pokemon.Base.Name}");

			//animamos el que sale
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(1.5f);
		}

		//configuramos el que entra
		playerUnit.SetupPokemon(newPokemon);

		battleDialogBox.SetPokemonMovements(newPokemon.Moves);

		playerUnit.PlayStartAnimation();

		yield return battleDialogBox.SetDialog($"Ve {newPokemon.Base.Name}");
		yield return new WaitForSeconds(1.0f);

		//estado para que avance
		if (previousState == null)
		{
			state = BattleState.RunTurn;
		}
		else if(previousState==BattleState.YesNoChoice)
		{
			yield return SendNextTrainerPokemonToBattle();
		}
	}

	IEnumerator ThrowPokeball()
	{
		//estado ocupado de la máquina de estados
		state = BattleState.Busy;

		if(type!=BattleType.WildPokemon)
		{
			yield return battleDialogBox.SetDialog("No puedes robar los pokemon de otros entrenadores");
			state = BattleState.RunTurn;
			//rompe la corrutina saliendo inmediatamente
			yield break;
		}

		//saca el nombre del objeto
		yield return battleDialogBox.SetDialog($"Has lanzado una {pokeball.name}!");

		SoundManager.SharedInstance.PlaySound(pokeballClip);

		//se instancia pokeball en posicion de battle unit, sin rotar
		//después se ajustó la posicion
		//estas posiciones son unidades de mundo, de unity
		GameObject pokeballInst = Instantiate(pokeball, 
								playerUnit.transform.position+new Vector3(-2,0,0),
								Quaternion.identity);

		SpriteRenderer pokeballSprite = pokeballInst.GetComponent<SpriteRenderer>();

		//argumentos
		//punto a donde queremos llegar ,fuerza, num saltos, duración
		//y esperamos hasta que se complete con WaitForCompletion
		//y como estamos en corrutina hacemos el yield return

		yield return pokeballSprite.transform.DOLocalJump(enemyUnit.transform.position + new Vector3(0, 1.5f),
											2f, 1, 1f).WaitForCompletion();

		//pokemon a la pokeball
		yield return enemyUnit.PlayCapturedAnimation();

		//pokeball cae
		yield return pokeballSprite.transform.DOLocalMoveY(enemyUnit.transform.position.y - 2.2f,
							1f).WaitForCompletion();

		int numberOfShakes = TryToCatchPokemon(enemyUnit.Pokemon);

		//hacemos un minimo para solo dejarlo hasta 3, el 4 era un valor para
		//capturar directo
		for (int i=0;i<Mathf.Min(numberOfShakes,3);i++)
		{
			yield return new WaitForSeconds(0.5f);

			yield return pokeballSprite.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.6f).WaitForCompletion();

		}

		if(numberOfShakes==4)
		{
			yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} capturado!");
			yield return pokeballSprite.DOFade(0, 1f).WaitForCompletion();

			if (playerParty.AddPokemonToParty(enemyUnit.Pokemon))
			{
				yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} añadido a tu equipo");
			}
			else
			{
				yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} se mandará al PC de BILL");
			}

			Destroy(pokeballInst);
			BattleFinish(true);			
		}
		else
		{
			//pokemon escapa
			yield return new WaitForSeconds(0.5f);
			pokeballSprite.DOFade(0, 0.2f);
			yield return enemyUnit.PlayBreakOutAnimation();
			if(numberOfShakes<2)
			{
				yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} ha escapado!");
			}
			else
			{
				yield return battleDialogBox.SetDialog("Casi lo has atrapado!");
			}

			Destroy(pokeballInst);

			state = BattleState.RunTurn;
		}
	}

	int TryToCatchPokemon(Pokemon pokemon)
	{
		float bonusPokeball = 1; //TODO clase pokeball con su multiplicador
		float bonusStatus = 1; //TODO stats para checar condiciones alteradas
		float a = (3 * pokemon.MaxHP - 2 * pokemon.Hp) * pokemon.Base.CatchRate * bonusPokeball * bonusStatus/(3*pokemon.MaxHP);
		
		if(a>=255)
		{
			return 4;
		}

		float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

		int shakeCount = 0;
		//para probar
		//shakeCount = 4;
		while(shakeCount<4)
		{
			if (Random.Range(0,65536)>=b)
			{
				break;
			}
			else
			{
				shakeCount++;
			}
		}

		return shakeCount;
	}
	
	IEnumerator TryToEscapeFromBattle()
	{
		state = BattleState.Busy;

		if(type!=BattleType.WildPokemon)
		{
			yield return battleDialogBox.SetDialog("No puedes huir de batallas contra entrenadores Pókemon");
			state = BattleState.RunTurn;
			yield break;
		}

		//otro intento de escape
		escapeAttempts++;

		int playerSpeed = playerUnit.Pokemon.Speed;
		int enemySpeed = enemyUnit.Pokemon.Speed;

		if (playerSpeed >= enemySpeed)
		{
			yield return battleDialogBox.SetDialog("Has escapado!!");
			yield return new WaitForSeconds(1.0f);
			BattleFinish(true);
		}
		else
		{
			int oddScape = (Mathf.FloorToInt(playerSpeed * 128/ enemySpeed) + 30 * escapeAttempts) % 256;
			if (Random.Range(0, 256) < oddScape)
			{
				yield return battleDialogBox.SetDialog("Has escapado!!");
				yield return new WaitForSeconds(1.0f);
				BattleFinish(true);
			}
			else
			{
				yield return battleDialogBox.SetDialog("No pudiste escapar...");
				state = BattleState.RunTurn;
			}
		}
	}

	IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
	{		
		yield return battleDialogBox.SetDialog($"{faintedUnit.Pokemon.Base.Name} se ha debilitado");
		SoundManager.SharedInstance.PlaySound(faintedClip);
		faintedUnit.PlayFaintAnimation();

		//esperamos que termine animación
		yield return new WaitForSeconds(1.5f);

		if(!faintedUnit.IsPlayer)
		{
			//ganar experiencia
			int expBase = faintedUnit.Pokemon.Base.ExpBase;
			int level = faintedUnit.Pokemon.Level;
			float multiplier = (type == BattleType.WildPokemon ? 1.0f : 1.5f);

			int wonExp = Mathf.FloorToInt(expBase * level * multiplier / 7);

			//sumamos experiencia
			playerUnit.Pokemon.Experience += wonExp;

			yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha ganado {wonExp} puntos de experiencia");
			yield return playerUnit.HUD.SetExpSmooth();

			yield return new WaitForSeconds(0.5f);			

			//checar new level
			while (playerUnit.Pokemon.NeedsToLevelUp())
			{
				SoundManager.SharedInstance.PlaySound(levelUpClip);
				yield return new WaitForSeconds(1.0f);

				playerUnit.HUD.SetLevelText();
				//se marca que cambió la vida
				playerUnit.Pokemon.HasHPChange = true;
				playerUnit.HUD.UpdatePokemonData();
				yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} sube de nivel");				

				//INTENTAR APRENDER NUEVO MOVIMIENTO
				LearnableMove newLearnableMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
				if(newLearnableMove!=null)
				{
					if (playerUnit.Pokemon.Moves.Count < PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
					{
						//se puede aprender
						playerUnit.Pokemon.LearnMove(newLearnableMove);

						yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha aprendido {newLearnableMove.Move.Name}");

						//actualizamos la información del pokemon en la interfaz
						battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
					}
					else
					{
						yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
						yield return battleDialogBox.SetDialog($"Pero no puede aprender más de {PokemonBase.NUMBER_OF_LEARNABLE_MOVES} movimientos");
						//el pokemon tiene que olvidar uno 
						yield return ChooseMovementToForget(playerUnit.Pokemon, newLearnableMove.Move);

						//esperar hasta que se cumpla una condición
						//parámetro de entrada ()
						//con la lambda se espera hasta que la condición se cumpla
						yield return new WaitUntil(() => state != BattleState.ForgetMovement);

						//se puede poner aquí el yield para ver el mensaje
						yield return new WaitForSeconds(1.0f);
					}
				}

				yield return playerUnit.HUD.SetExpSmooth(true);

			}
		}

		CheckForBattleFishish(faintedUnit);
	}

	IEnumerator ChooseMovementToForget(Pokemon learner, MoveBase newMove)
	{
		//el estado es para que no tenga control el usuario mientras se muestra interfaz
		state = BattleState.Busy;

		yield return battleDialogBox.SetDialog("Selecciona el movimiento que quieres olvidar...");
		//activamos la interfaz en pantalla, recordar que era una componente de un game object
		moveSelectionUI.gameObject.SetActive(true);

		//los moves se filtran para obtener los base
		moveSelectionUI.SetMovements(learner.Moves.Select(mv =>mv.Base).ToList(), newMove);

		//respaldamos el movimiento a aprender para responder a la UI
		moveToLearn = newMove;

		//ahora ya podemos subir y bajar en el panel
		state = BattleState.ForgetMovement;


	}

}
