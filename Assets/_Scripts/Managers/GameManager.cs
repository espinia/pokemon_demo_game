using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
	//jugador moviendose en el mapa
	Travel,
	Battle,
	Dialog,
	Cutscene
}

//siempre se agregará el ColorManager
[RequireComponent(typeof(ColorManager))]
[RequireComponent(typeof(DialogManager))]
public class GameManager : MonoBehaviour
{
	[SerializeField]
	PlayerController playerController;

	[SerializeField]
	BattleManager battleManager;

	[SerializeField]
	Camera worldMainCamera;

	//para la transición
	[SerializeField]
	Image transitionPanel;

	GameState _gameState;

	public AudioClip worldClip, battleClip;

	public static GameManager SharedInstance;

	private TrainerController trainer;

	private void Awake()
	{
		if (SharedInstance == null)
		{
			SharedInstance = this;
		}
		else
		{
			Destroy(this);
		}
		_gameState = GameState.Travel;
	}

	private void Start()
	{
		//inicializamos la factory de las status condition
		StatusConditionFactory.InitFactory();

		SoundManager.SharedInstance.PlayMusic(worldClip);

		//agregamos el listener
		playerController.OnPokemonEncountered += StartPokemonBattle;

		playerController.OnEnterTrainersFOV+= (Collider2D trainerCollider)=>
		{

			//pedimos al padre la componente
			TrainerController trainer= trainerCollider.GetComponentInParent<TrainerController>();
			if (trainer != null)
			{
				//cambiamos el estado para congelar al player
				_gameState = GameState.Cutscene;
				StartCoroutine(trainer.TriggerTrainerBattle(playerController));
			}
		};

		battleManager.OnBattleFinish += FinishPokemonBattle;

		//definimos aqui mismo que hace
		DialogManager.SharedInstance.OnDialogStart += ()=>{
			_gameState = GameState.Dialog;
		};

		//definimos aqui mismo que hace
		DialogManager.SharedInstance.OnDialogFinish += () => {
			//puede ir a estado travel o si estamos hablando
			//con un entrenador ir a estado battle
			if (_gameState == GameState.Dialog)
			{
				_gameState = GameState.Travel;
			}
		};
	}

	void StartPokemonBattle()
	{
		this.trainer = null;
		StartCoroutine(FadeInBattle());
	}

	public void StartTrainerBattle(TrainerController trainer)
	{
		this.trainer = trainer;
		StartCoroutine(FadeInTrainerBattle(trainer));
	}


	IEnumerator FadeInBattle()
	{
		SoundManager.SharedInstance.PlayMusic(battleClip);

		_gameState = GameState.Battle;

		//hacemos el fade
		yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
		//se queda en negros un momento
		yield return new WaitForSeconds(0.2f);

		//es una componente y vamos al objeto que la contiene
		//como es hija la cámara también la activa
		battleManager.gameObject.SetActive(true);

		//desactivamos cámara principal
		//también es una componente
		worldMainCamera.gameObject.SetActive(false);

		PokemonParty playerParty = playerController.GetComponent<PokemonParty>();
		//si solo tenemos un área y en plural si tenemos varias para ver en cual estamos
		//con distancia, colision enter , etc
		//ubicamos objeto, sacamos la componente, y luego invocamos método
		Pokemon wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().GetRandomWildPokemon();

		//hacemos copia del salvaje
		Pokemon wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);				

		//es el que mandamos a batalla para que no sea el mismo
		//arrancando la batalla
		battleManager.HandleStartBattle(playerParty, wildPokemonCopy);

		//hacemos el fade a transparente ya que la cámara nueva está encendida
		yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
	}

	IEnumerator FadeInTrainerBattle(TrainerController trainer)
	{
		SoundManager.SharedInstance.PlayMusic(battleClip);

		_gameState = GameState.Battle;

		//hacemos el fade
		yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
		//se queda en negros un momento
		yield return new WaitForSeconds(0.2f);

		//es una componente y vamos al objeto que la contiene
		//como es hija la cámara también la activa
		battleManager.gameObject.SetActive(true);

		//desactivamos cámara principal
		//también es una componente
		worldMainCamera.gameObject.SetActive(false);

		PokemonParty playerParty = playerController.GetComponent<PokemonParty>();

		//obtenemos la party del rival
		PokemonParty trainerParty = trainer.GetComponent<PokemonParty>();

		//es el que mandamos a batalla para que no sea el mismo
		//arrancando la batalla
		battleManager.HandleStartTrainerBattle(playerParty, trainerParty);

		//hacemos el fade a transparente ya que la cámara nueva está encendida
		yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
	}

	void FinishPokemonBattle(bool playerHasWon)
	{
		//entrenador perdió batalla
		if(trainer!=null && playerHasWon)
		{
			trainer.AfterLostBattle();
			trainer = null;
		}

		StartCoroutine(FadeOutBattle());
	}
	IEnumerator FadeOutBattle()
	{
		//fade a negro
		yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
		//se queda en negros un momento
		yield return new WaitForSeconds(0.2f);
		
		//desactivamos
		battleManager.gameObject.SetActive(false);
		//cámara regresa
		worldMainCamera.gameObject.SetActive(true);

		yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();

		//recupera sonido de la pantalla de mundo
		SoundManager.SharedInstance.PlayMusic(worldClip);

		//tiene parámetro para capturar el dato enviado por el evento
		//regresamos a travel
		_gameState = GameState.Travel;
	}

	private void Update()
	{
		if (_gameState == GameState.Travel)
		{
			playerController.HandleUpdate();
		}
		else if (_gameState == GameState.Battle)
		{
			battleManager.HandleUpdate();
		}
		else if (_gameState == GameState.Dialog)
		{
			DialogManager.SharedInstance.HandleUpdate();
		}
	}
}
