using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class BattleUnit : MonoBehaviour
{
	//Base del pokemon
	public PokemonBase _base;
	public int _level;

	[SerializeField]
	BattleHUD hud;

	public BattleHUD HUD => hud;

	[SerializeField]
	bool isPlayer;

	public bool IsPlayer
	{
		get => isPlayer;
	}

	//se asigna y se obtiene el mismo pokemon
	//por si hay un procesamiento en alguno de los 2
	public Pokemon Pokemon { get; set; }

	//referencia a sprite
	Image pokemonImage;

	//referencia a posición donde está el pokemon
	//al iniciar batalla
	private Vector3 initialPosition;

	//color original
	private Color initialColor;

	[SerializeField]
	private float startTimeAnim=1.0f, attackTimeAnim=0.3f, 
				  dieTimeAnim=1.0f,hitTimeAnim=0.15f,
				  captureTimeAnim=0.6f;

	private void Awake()
	{
		pokemonImage = GetComponent<Image>();
		//se toma la local porque es respecto al padre
		//se hace respecto a como está anclado y posicionado al canvas
		initialPosition = pokemonImage.transform.localPosition;

		initialColor = pokemonImage.color;
	}
	public void SetupPokemon(Pokemon pokemon)
	{
		//primera vérsión para probar
		//Pokemon=new Pokemon(_base, _level);
		Pokemon = pokemon;

		/* Abajo está simplificado
		if(isPlayer)
		{
			//hay que cambiar la imagen de este componente
			//por el de espalda
			GetComponent<Image>().sprite = Pokemon.Base.BackSprite;
		}
		else
		{
			//si no es el del player se puede poner el del frente
			GetComponent<Image>().sprite = Pokemon.Base.FrontSprinte;
		}*/
		//o con el operador de una linea
		//caso verdadero primero, luego caso falso
		pokemonImage.sprite = 
			(isPlayer ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite);

		//regresamos el sprite al principio reiniciando tras cada batalla
		pokemonImage.color = initialColor;
		pokemonImage.transform.position = initialPosition;

		//actualizamos el hud
		hud.gameObject.SetActive(true);
		hud.SetPokemonData(pokemon);

		transform.localScale = new Vector3(1, 1, 1);

		PlayStartAnimation();
	}

	public void PlayStartAnimation()
	{
		/*
		if(isPlayer)
		{
			//lo sacamos de la pantalla
			pokemonImage.transform.localPosition = new Vector3(initialPosition.x-400, initialPosition.y);
		}
		else
		{
			//lo sacamos de la pantalla
			pokemonImage.transform.localPosition = new Vector3(initialPosition.x + 400, initialPosition.y);
		}
		*/
		//o con el ternario
		float newPosition = initialPosition.x + (isPlayer ? -1 : 1) * 400;
		pokemonImage.transform.localPosition = new Vector3(newPosition, initialPosition.y);

		//se usa la librearía
		//primer parámetro punto final
		//segundo parámetro cuanto dura en segundos
		pokemonImage.transform.DOLocalMoveX(initialPosition.x, startTimeAnim);		
	}

	public void PlayAttackAnimation()
	{
		Sequence seq = DOTween.Sequence();

		//Va
		float finalPos = initialPosition.x + (isPlayer ? 1 : -1) * 60;
		seq.Append(pokemonImage.transform.DOLocalMoveX(finalPos, attackTimeAnim));

		//regresa
		seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, attackTimeAnim));


	}

	public void PlayReceiveAttackAnimation()
	{
		Sequence seq = DOTween.Sequence();

		//a gris
		seq.Append(pokemonImage.DOColor(Color.gray, hitTimeAnim));
		//regresa a color normal
		seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnim));
	}

	public void PlayFaintAnimation()
	{
		Sequence seq = DOTween.Sequence();

		//movimiento hacia abajo
		seq.Append(pokemonImage.transform.DOLocalMoveY(initialPosition.y - 200, dieTimeAnim));
		//se une la siguiente fundiendo
		//primer parámetro valor final del alpha, segundo parámetro tiempo
		seq.Join(pokemonImage.DOFade(0.0f,dieTimeAnim));		
	}

	public IEnumerator PlayCapturedAnimation()
	{
		Sequence seq = DOTween.Sequence();

		//fadeOut
		pokemonImage.DOFade(0.0f, captureTimeAnim);

		//escalar,sobre el transform del objeto
		//la z en 1 o desaparecera
		seq.Join(transform.DOScale(new Vector3(0.25f,0.25f,1),captureTimeAnim)); 

		//aqui si las unidades son pixeles
		seq.Join(transform.DOLocalMoveY(initialPosition.y + 50, captureTimeAnim));

		//esperamos a que termine
		yield return seq.WaitForCompletion();

	}

	public IEnumerator PlayBreakOutAnimation()
	{
		Sequence seq = DOTween.Sequence();

		//fadeOut
		pokemonImage.DOFade(1.0f, captureTimeAnim);

		//escalar,sobre el transform del objeto
		seq.Join(transform.DOScale(new Vector3(1,1,1), captureTimeAnim));

		//aqui si las unidades son pixeles
		seq.Join(transform.DOLocalMoveY(initialPosition.y, captureTimeAnim));

		//esperamos a que termine
		yield return seq.WaitForCompletion();

	}

	public void ClearHUD()
	{
		hud.gameObject.SetActive(false);
	}
}
