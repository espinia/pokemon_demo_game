using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Move",menuName ="Pokemon/Nuevo Movimiento")]
public class MoveBase : ScriptableObject
{
	[SerializeField]
	private string _name;
	public string Name => _name;

	[TextArea]
	[SerializeField]
	private string description;
	public string Description => description;

	//naturaleza del ataque
	[SerializeField]
	private PokemonType type;
	public PokemonType Type => type;

	//tipo del movimiento
	[SerializeField]
	private MoveType moveType;
	public MoveType MoveType => moveType;

	[SerializeField]
	private MoveStatEffect effects;
	public MoveStatEffect Effects=> effects;

	//efectos secundarios
	[SerializeField] List<SecondaryMoveStatEffect> secondaryEffects;
	public List<SecondaryMoveStatEffect> SecondaryEffects => secondaryEffects;

	[SerializeField]
	private MoveTarget target;
	public MoveTarget Target =>target;

	[SerializeField]
	private int power;
	public int Power => power;

	//tasa de exito
	[SerializeField]
	private int accuracy;
	public int Accuracy => accuracy;
	
	//check siempre hit
	[SerializeField]
	private bool alwaysHit;
	public bool AlwaysHit => alwaysHit;

	//power points antes de recargar
	[SerializeField]
	private int pp;
	public int Pp => pp;

	[SerializeField]
	private int priority=0;
	public int Priority => priority;

	//para regresar si esta marcado como especial o no
	//como antes con bool
	public bool IsSpecialMove => moveType == MoveType.Special;
			/*Primera versión
			if (type == PokemonType.Fire ||
				type == PokemonType.Water ||
				type == PokemonType.Grass ||
				type == PokemonType.Ice ||
				type == PokemonType.Electric ||
				type == PokemonType.Dragon ||
				type == PokemonType.Dark ||
				type == PokemonType.Psycho
				)
			{
				return true;
			}
			else
			{
				return false;
			}*/
}

//tipos de ataque
public enum MoveType
{
	Physical,
	Special,
	Stats
}

//serializable para el scriptable object
[Serializable]
public class MoveStatEffect
{
	//alteración de boostings
	[SerializeField] List<StatBoosting> boostings;
	//para estados alterados normales
	[SerializeField] StatusConditionID status;
	
	//para estados alterados volátiles
	[SerializeField] StatusConditionID volatileStatus;
	public List<StatBoosting> Boostings => boostings;

	public StatusConditionID Status => status;
	public StatusConditionID VolatileStatus => volatileStatus;

}

//serializable para el scriptable object
[Serializable]
//: Herencia
public class SecondaryMoveStatEffect:MoveStatEffect
{
	//probabilidad
	[SerializeField] int chance;
	[SerializeField] MoveTarget target;

	public int Chance => chance;
	public MoveTarget Target => target;
}


[System.Serializable]
public class StatBoosting
{
	//que estado modifica?
	public Stat stat;
	//tipo de mejora (positiva, negativa)
	public int boost;

	public MoveTarget target;
}

public enum MoveTarget
{
	//yo  //contrario
	Self, Other
}