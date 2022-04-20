using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonMapArea : MonoBehaviour
{
	[SerializeField]
	private List<Pokemon> wildPokemons;


	//regresamos pokemon aleatorio
	//podrían tener un peso para manejar diferente probabilidad
	//y hacer el range sobre la suma de los pesos
	//o agregar más pokemon de un tipo a la lista desde el editor
	public Pokemon GetRandomWildPokemon()
	{
		Pokemon pokemon= wildPokemons[Random.Range(0, wildPokemons.Count)];
		pokemon.InitPokemon();

		return pokemon;
	}

}
