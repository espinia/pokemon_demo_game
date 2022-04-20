using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    //using UnityEngine.UI;
	//Texto para mostrar diálogos
    [SerializeField]
	Text dialogText;

	//para conectar las otras partes de la caja
	[SerializeField]
	GameObject actionSelect;

	[SerializeField]
	GameObject movementSelect;

	[SerializeField]
	GameObject movementDesc;

	//lista de textos para las acciones
	[SerializeField]
	List<Text> actionTexts;

	//lista de textos para los movimientos de los pokemon
	[SerializeField]
	List<Text> movementTexts;

	//para los pp
	[SerializeField]
	Text ppText;

	//para los pp
	[SerializeField]
	Text typeText;

	//para los pp
	[SerializeField]
	Image backgroundTypeText;

	//para yes no
	[SerializeField]
	GameObject yesNoBox;

	[SerializeField]
	Text yesText;

	[SerializeField]
	Text noText;


	public float characterPerSecond=10.0f;

	public float timeWaitAfterText = 1.0f;

	public bool isWriting = false;

	/* versión que arranca él mismo la escritura
    public void SetDialog(string message)
	{
		//de golpe
		//dialogText.text = message;
		StartCoroutine(WriteDialog(message));
	}*/

	//corrutina para el texto
	public IEnumerator SetDialog(string message)
	{
		isWriting = true;

		dialogText.text = "";

		//para devolver letra a letra
		foreach (var character in message)
		{
			dialogText.text += character;
			//se podría poner el sonido
			if(character!=' ')
			{
				SoundManager.SharedInstance.RandomCharacterSoundEffect();
			}

			//espera a que retorne tantos segundos
			//segundos solo es en tiempo de juego
			//y real, es en segundos del mundo real
			yield return new WaitForSeconds(1 / characterPerSecond);
		}

		//esperamos tiempo a que se lea el mensaje
		yield return new WaitForSeconds(timeWaitAfterText);

		isWriting = false;
	}
	public void ToggleDialogText(bool activated)
	{
		//para el dialog text es enable
		dialogText.enabled = activated;
	}

	public void ToggleActions(bool activated)
	{
		actionSelect.SetActive(activated);
	}

	public void ToggleMovements(bool activated)
	{
		movementSelect.SetActive(activated);
		movementDesc.SetActive(activated);
	}

	public void SelectAction(int selectedAction)
	{
		//el ciclo resalta la que necesitamos
		//las otras vuelven a la normalidad
		for(int i = 0; i < actionTexts.Count; i++)
		{
			/*
			if(i==selectedAction)
			{
				actionTexts[i].color = selectedColor;
			}
			else
			{
				actionTexts[i].color = Color.black;
			}*/
			/*o de esta forma*/
			actionTexts[i].color = (i == selectedAction ? ColorManager.SharedInstance.selectedColor: ColorManager.SharedInstance.defaultColor);
		}
	}

	public void SetPokemonMovements(List<Move> moves)
	{
		//cambiamos los textos de las cajas
		for (int i=0;i<movementTexts.Count;i++)
		{
			if(i<moves.Count)
			{
				//hay un movimiento para colocar
				movementTexts[i].text = moves[i].Base.Name;
			}
			else
			{
				movementTexts[i].text = "---";
			}
		}
	}

	public void SelectMovement(int selectedMovement, Move move)
	{	
		//el ciclo resalta el move que necesitamos
		//los otros vuelven a la normalidad
		for (int i = 0; i < movementTexts.Count; i++)
		{
			movementTexts[i].color = (i == selectedMovement ? ColorManager.SharedInstance.selectedColor : ColorManager.SharedInstance.defaultColor);
		}

		//actualizamos la descripción de al lado en la interfaz
		ppText.text = $"PP {move.Pp}/{move.Base.Pp}";
		
		//convertimos enumerado a cadena
		typeText.text = move.Base.Type.ToString().ToUpper();

		//ppText.color = (move.Pp <= 0 ? Color.red : Color.black);
		ppText.color = ColorManager.SharedInstance.GetPPColor((float)move.Pp/move.Base.Pp);

		//color para el texto de tipo
		backgroundTypeText.color=ColorManager.TypeColor.GetColorFromType(move.Base.Type);

	}

	public void ToggleYesNoBox(bool activated)
	{
		yesNoBox.SetActive(activated);
	}

	public void SelectYesNoAction(bool yesSelected)
	{
		if(yesSelected)
		{
			yesText.color = ColorManager.SharedInstance.selectedColor;
			noText.color = ColorManager.SharedInstance.defaultColor;
		}
		else
		{
			noText.color = ColorManager.SharedInstance.selectedColor;
			yesText.color = ColorManager.SharedInstance.defaultColor;
		}		 
	}
}

