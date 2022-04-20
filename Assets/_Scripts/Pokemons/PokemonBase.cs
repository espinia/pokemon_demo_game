using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Pokemon",menuName ="Pokemon/Nuevo Pokemon")]
public class PokemonBase : ScriptableObject
{
	[SerializeField]
	private int id;

	[SerializeField]
	private string name;
	public string Name => name;

	[TextArea] //para que sea un espacio grande en el editor
	[SerializeField]
	private string description;
	public string Description => description;

	//sprite del frente
	[SerializeField]
	private Sprite frontSprite;
	public Sprite FrontSprite { get => frontSprite; }

	//sprite de detrás
	[SerializeField]
	private Sprite backSprite;
	public Sprite BackSprite { get => backSprite; }

	[SerializeField]
	private PokemonType type1;
	public PokemonType Type1 => type1;

	[SerializeField]
	private PokemonType type2;
	public PokemonType Type2 => type2;

	//Stats
	[SerializeField]
	private int maxHP;
	public int MaxHP => maxHP;

	[SerializeField]
	private int attack;
	public int Attack => attack;

	[SerializeField]
	private int defense;
	public int Defense => defense;

	[SerializeField]
	private int spAttack;
	public int SpAttack => spAttack;

	[SerializeField]
	private int spDefense;
	public int SpDefense => spDefense;

	[SerializeField]
	private int speed;
	public int Speed => speed;

	[SerializeField]
	private int expBase;

	public int ExpBase => expBase;

	[SerializeField]
	GrowthRate growthRate;
	public GrowthRate GrowthRate=>growthRate;

	[SerializeField]
	//movimientos que puede aprender
	private List<LearnableMove> learnableMoves;
	public List<LearnableMove>LearnableMoves => learnableMoves;

	[SerializeField]
	private int catchRate=255;

	public int CatchRate => catchRate;

	//solo se puede leer por el get
	public static int NUMBER_OF_LEARNABLE_MOVES { get; } = 4;
	public int GetNecesaryExpForLevel(int level)
	{
		switch(growthRate)
		{
			case GrowthRate.Fast:
				return Mathf.FloorToInt(4 * Mathf.Pow(level, 3) / 5);
			case GrowthRate.MediumFast:
				return Mathf.FloorToInt( Mathf.Pow(level, 3));
			case GrowthRate.MediumSlow:
				return Mathf.FloorToInt(6*Mathf.Pow(level, 3)/5-15*Mathf.Pow(level,2)+100*level-140);
			case GrowthRate.Slow:
				return Mathf.FloorToInt(5 * Mathf.Pow(level, 3) / 4);
			case GrowthRate.Erratic:
				if(level<50)
				{
					return Mathf.FloorToInt( Mathf.Pow(level, 3) *(100-level)/50);
				}
				else if(level<68)
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * (150 - level) / 100);
				}
				else if (level<98)
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * Mathf.FloorToInt((1911 - 10*level) / 3)/500);
				}
				else
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * (160 - level) / 100);
				}				
			case GrowthRate.Fluctuating:
				if (level < 15)
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * (Mathf.FloorToInt((level+1)/3)+24)/50);
				}
				else if(level<36)
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level + 14) / 50);
				}
				else
				{
					return Mathf.FloorToInt(Mathf.Pow(level, 3) * (Mathf.FloorToInt(level/2) +32) / 50);
				}				
		}
		//caso default no debería suceder
		return -1;
	}
}

public enum GrowthRate
{
	Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
}
public enum PokemonType
{
	None,
	Normal,
	Fire,
	Water,
	Electric,
	Grass,
	Ice,
	Fight,	
	Poison,
	Ground,
	Fly,
	Psycho,
	Bug,
	Rock,	
	Ghost,
	Dragon,
	Dark,
	Fairy,
	Steel
}

public enum Stat
{
	Attack,
	Defense,
	SpAttack,
	SpDefense,
	Speed,
	Accuracy,
	Evasion
}

public class TypeMatrix
{
	//llegué hasta ice
	private static float[][] matrix =
	{
							//NOR  FIR  WAT  ELE  GRA  ICE  FIG  POI  GRO  FLY  PSY  BUG  ROC  GHO  DRA  DAR  STE  FAI
		/*NOR*/new float[]	{  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,0.5f,  0f,  1f,  1f,0.5f,  1f},
		/*FIR*/new float[]  {  1f,0.5f,0.5f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f,0.5f,  1f,0.5f,  1f,  2f,  1f},
		/*WAT*/new float[]  {  1f,  2f,0.5f,  1f,0.5f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f,0.5f,  1f,  1f,  1f},
		/*ELE*/new float[]  {  1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f,0.5f,  1f,  1f,  1f},
		/*GRA*/new float[]  {  1f,0.5f,  2f,  1f,0.5f,  1f,  1f,0.5f,  2f,0.5f,  1f,0.5f,  2f,  1f,0.5f,  1f,0.5f,  1f},
		/*ICE*/new float[]  {  1f,0.5f,0.5f,  1f,  2f,0.5f,  1f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  2f,  1f,0.5f,  1f},
		/*FIG*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*POI*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*GRO*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*FLY*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*PSY*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*BUG*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*ROC*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*GHO*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*DRA*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*DAR*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*STE*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
		/*FAI*/new float[]  {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f},
	};

	//no depende de instanciar por ser static
	//attackType == tipo ataque
	//pokemonDefenderType == tipo pokemon
	public static float GetMultiplierEffectiveness(PokemonType attackType, PokemonType pokemonDefenderType)
	{
		//si metemos el none en la matriz ya no es necesario, ni restar las posiciones
		if(attackType == PokemonType.None || pokemonDefenderType == PokemonType.None)
		{
			return 1.0f;
		}
		else
		{
			//obtenemos posicion dentro de enumerado con un casting
			int row = (int)attackType - 1;
			int col = (int)pokemonDefenderType - 1;

			return matrix[row][col];
		}
	}


}

[Serializable]
public class LearnableMove
{
[SerializeField]
//alguno de los movimientos definidos
private MoveBase move;
public MoveBase Move => move;

[SerializeField]
private int level;
public int Level => level;
}