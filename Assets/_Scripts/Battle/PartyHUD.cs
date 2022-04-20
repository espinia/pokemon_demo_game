using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHUD : MonoBehaviour
{
	[SerializeField]
	private Text messageText;


	//no crece o decrece
	private PartyMemberHUD[] memberHUDs;

	private List<Pokemon> pokemons;

	public void InitPartyHUD()
	{
		//nos da todas las componentes en el hijo, pero solo las activas, por default
		//si le pasamos el true también tendremos las inactivas
		memberHUDs = GetComponentsInChildren<PartyMemberHUD>(true);
	}

	//muestra la info de la lista de pokemon
	public void SetPartyData(List <Pokemon>pokemons)
	{
		this.pokemons = pokemons;
		messageText.text = "Selecciona un pokemon...";
		//para cada caja
		for (int i = 0; i < memberHUDs.Length; i++)
		{
			if (i < pokemons.Count)
			{
				memberHUDs[i].SetPokemonData(pokemons[i]);
				memberHUDs[i].gameObject.SetActive(true);
			}
			else
			{
				//esa caja la quitamos
				memberHUDs[i].gameObject.SetActive(false);
			}
		}
	}

	public void UpdateSelectedPokemon(int selectedPokemon)
	{
		for (int i = 0; i < pokemons.Count; i++)
		{
			/* o de la otra forma
			if (i == selectedPokemon)
			{
				memberHUDs[i].SetSelectedPokemon(true);
			}
			else
			{
				//esa caja la quitamos
				memberHUDs[i].SetSelectedPokemon(false);
			}*/
			//la comparacion ya genera el true o false
			memberHUDs[i].SetSelectedPokemon(i==selectedPokemon);
		}
	}

	public void SetMessage(string msg)
	{
		messageText.text = msg;
	}
}
