using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColorManager : MonoBehaviour
{
	public static ColorManager SharedInstance;
	public Color selectedColor;
	public Color defaultColor=Color.black;

	[SerializeField]
	Color clRedBar;

	[SerializeField]
	Color clYellowBar;

	[SerializeField]
	Color clGreenBar;

	private void Awake()
	{
		SharedInstance = this;	
	}

	public Color GetBarColor(float finalScale)
	{
		if (finalScale < 0.20)
		{
			return clRedBar;
		}
		else if (finalScale < 0.5)
		{
			return clYellowBar;
		}
		else
		{
			//asi se trabajaría con colores
			//new Color(98f / 255, 178 / 255, 61 / 255);
			return clGreenBar;
		}
	}

	public Color GetPPColor(float finalScale)
	{
		if (finalScale < 0.2)
		{
			return clRedBar;
		}
		else if (finalScale < 0.5)
		{
			return clYellowBar;
		}
		else
		{
			//asi se trabajaría con colores
			//new Color(98f / 255, 178 / 255, 61 / 255);
			return Color.black;
		}
	}

	public class TypeColor
	{
		private static Color[] colors =
		{
			Color.white ,//none
			new Color(0.873f,0.873f,0.873f),//Normal,
			new Color(0.999f,0.596f,0.528f),//Fire,
			new Color(0.561f,0.783f,1.0f),//Water,
			new Color(0.873f,0.873f,0.873f),//Electric,
			new Color(0.41f,1.0f,0.685f),//Grass,
			new Color(0.873f,0.873f,0.873f),//Ice,
			new Color(0.873f,0.873f,0.873f),//Fight,
			new Color(0.873f,0.873f,0.873f),//Poison,
			new Color(0.873f,0.873f,0.873f),//Ground,
			new Color(0.873f,0.873f,0.873f),//Fly,
			new Color(0.873f,0.873f,0.873f),//Psycho,
			new Color(0.873f,0.873f,0.873f),//Bug,
			new Color(0.873f,0.873f,0.873f),//Rock,
			new Color(0.873f,0.873f,0.873f),//Ghost,
			new Color(0.656f,0.557f,0.764f),//Dragon,
			new Color(0.873f,0.873f,0.873f),//Dark,
			new Color(0.873f,0.873f,0.873f),//Fairy,
			new Color(0.873f,0.873f,0.873f),//Steel
		};
		public static Color GetColorFromType(PokemonType type)
		{
			return colors[(int)type];
		}
	}

	public class StatusConditionColor
	{
		//recordar el constructor
		static Dictionary<StatusConditionID, Color> colors = new Dictionary<StatusConditionID, Color>
		{
			{StatusConditionID.none,Color.white },
			{StatusConditionID.brn,new Color(233.0f/255,134f/255,67f/255)},
			{StatusConditionID.frz,new Color(168.0f/255,214f/255,215f/255)},
			{StatusConditionID.par,new Color(241.0f/255,208f/255,83f/255)},
			{StatusConditionID.psn,new Color(147.0f/255,73f/255,156f/255)},
			{StatusConditionID.slp,new Color(163.0f/255,147f/255,234f/255)}
		};

		public static Color GetColorFromStatusCondition(StatusConditionID id)
		{
			return colors[id];
		}
	}
}
