using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PartyMemberHUD : MonoBehaviour
{
	//para conectar
	[SerializeField]
	Text nameText, levelText, typeText;
	[SerializeField]
	HealthBar healthBar;
	[SerializeField]
	Image pokemonImage;

	Image _baseImage;

	Pokemon _pokemon;

	public void SetPokemonData(Pokemon pokemon)
	{
		_pokemon = pokemon;

		nameText.text = pokemon.Base.name;
		levelText.text = $"Lv. {pokemon.Level}";
		//recordar porque es enumerado
		if (pokemon.Base.Type2 != PokemonType.None)
		{
			typeText.text = $"{pokemon.Base.Type1.ToString().ToUpper()}/{pokemon.Base.Type2.ToString().ToUpper()} ";
		}
		else
		{
			typeText.text = pokemon.Base.Type1.ToString().ToUpper();
		}

		healthBar.SetHP(pokemon);

		pokemonImage.sprite = pokemon.Base.FrontSprite;

		//ponemos el color
		if (_baseImage == null)
		{
			_baseImage = GetComponent<Image>();
			//Debug.Log("Base null");
		}

		_baseImage.color=ColorManager.TypeColor.GetColorFromType(pokemon.Base.Type1);
	}
	public void SetSelectedPokemon(bool selected)
	{
		//resaltamos si debe estar seleccionado
		if (selected)
		{
			nameText.color = ColorManager.SharedInstance.selectedColor;
		}
		else
		{
			nameText.color = Color.black;
		}
	}
}
