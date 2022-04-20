using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField]
    Text[] movementTexts;

	int currentSelectedMovement;

	private void Awake()
	{
		//obtenemos los hijos en el awake, porque no hay start si no está activo
		movementTexts = GetComponentsInChildren<Text>(true);
	}

	public void SetMovements(List<MoveBase>pokemomMoves,MoveBase newMove)
	{
		currentSelectedMovement = 0;

		for (int i=0;i< pokemomMoves.Count;i++)
		{			
			movementTexts[i].text = pokemomMoves[i].Name;
		}

		movementTexts[pokemomMoves.Count].text = newMove.Name;
		UpdateColorForgetMoveSelection();
	}

	public void HandleForgetMoveSelection(Action<int> onSelected)
	{
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			int direction = (int)Input.GetAxisRaw("Vertical");
			currentSelectedMovement -= direction;
			//currentSelectedMovement = currentSelectedMovement % (PokemonBase.NUMBER_OF_LEARNABLE_MOVES+1);
			currentSelectedMovement = Mathf.Clamp(currentSelectedMovement, 0,PokemonBase.NUMBER_OF_LEARNABLE_MOVES);

			UpdateColorForgetMoveSelection();

			onSelected.Invoke(-1);
		}
		else if (Input.GetAxisRaw("Submit")!=0)
		{
			onSelected?.Invoke(currentSelectedMovement);
		}

	}

	public void UpdateColorForgetMoveSelection()
	{
		for (int i = 0; i < movementTexts.Length; i++)
		{
			movementTexts[i].color = ((currentSelectedMovement == i) ? ColorManager.SharedInstance.selectedColor : Color.black);
		}
	}
}
