using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    //para las colisiones al moverme
    [SerializeField]
    LayerMask solidObjectsLayer;

    //para las capas donde aparecen pokemo, no chocamos será trigger
    [SerializeField]
    LayerMask pokemonLayer;

    //para objetos interactuables
    [SerializeField]
    LayerMask interactableLayer;

    //para el player
    [SerializeField]
    LayerMask playerLayer;

    //para campos de visión
    [SerializeField]
    LayerMask fovLayer;

    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask PokemonLayer => pokemonLayer;
    public LayerMask InteractableLayer => interactableLayer;

    public LayerMask PlayerLayer => playerLayer;

    public LayerMask FovLayer => fovLayer;

    public static GameLayers SharedInstance;

	private void Awake()
	{
		if(SharedInstance==null)
		{
            SharedInstance = this;
		}
	}

    public LayerMask CollisionLayers =>
        GameLayers.SharedInstance.SolidObjectsLayer | //capas para chocar
        GameLayers.SharedInstance.InteractableLayer |
        GameLayers.SharedInstance.PlayerLayer;
}

