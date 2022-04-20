using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
	[SerializeField]
	private List<Pokemon> pokemons;

	//constante
	public const int NUM_MAX_POKEMON_IN_PARTY = 6;

	//ejemplo
	//private List<List<Pokemon>> pcBillBoxes;

	public List<Pokemon>Pokemons
	{
		get => pokemons;
		//no se usará
		//set => pokemons = value;
	}

	private void Start()
	{
		//se inicializan para la batalla
		foreach(Pokemon pokemon in pokemons)
		{
			pokemon.InitPokemon();
		}

		/*
		for (int i=0;i<6;i++)
		{
			List<Pokemon> box = new List<Pokemon>(15);
			pcBillBoxes.Add(box);
		}*/
	}

	public Pokemon GetFirstNonFaintedPokemon()
	{
		//se puede hacer con un ciclo
		//que busque el primero con vida >0

		//o funcion lambda
		//esto hace que se itere sobre la lista con la variable p
		//y quedaros con el que tiene HP>0
		//FirstOrDefault para obtener el primero o default
		return pokemons.Where(p => p.Hp > 0).FirstOrDefault();

	}

	public int GetPositionFromPokemon(Pokemon pokemon)
	{
		for (int i = 0; i < Pokemons.Count; i++)
		{
			if (Pokemons[i] == pokemon)
			{
				return i;
			}
		}

		return 0;
	}

	public bool AddPokemonToParty(Pokemon pokemon)
	{
		if (pokemons.Count < NUM_MAX_POKEMON_IN_PARTY)
		{
			//hay espacio
			pokemons.Add(pokemon);
			return true;
		}
		else
		{
			//TODO : añadir funcionalidad de enviar a PC
			return false;
		}
	}
}
