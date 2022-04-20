using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
	[SerializeField]
	GameObject dialogBox;

	[SerializeField]
	Text dialogText;

	[SerializeField]
	float characterPerSecond = 30.0f;

	public static DialogManager SharedInstance;

	public event Action OnDialogStart, OnDialogFinish;

	//para detectar lo rápido que presionan las teclas
	float timeSinceLastClick;
	[SerializeField]
	float timeBetweenClicks = 1.0f;

	int currentLine = 0;
	Dialog currentDialog;
	bool isWriting = false;

	public bool IsBeingShown = false;
	public Action OnDialogClose;

	private void Awake()
	{
		if(SharedInstance==null)
		{
			SharedInstance = this;
		}
		
	}
	public void ShowDialog(Dialog dialog, Action onDialogFinish=null)
	{
		//se invoca el evento, para el game manager
		OnDialogStart?.Invoke();		

		//mostramos la caja
		dialogBox.SetActive(true);
		currentLine = 0;
		currentDialog = dialog;
		IsBeingShown = true;

		//respaldamos la referencia a la accion a ajecutar al terminar el dialogo
		this.OnDialogClose = onDialogFinish;
		StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));

	}

	public IEnumerator SetDialog(string line)
	{
		isWriting = true;

		dialogText.text = "";

		//para devolver letra a letra
		foreach (var character in line)
		{
			dialogText.text += character;
			//se podría poner el sonido
			if (character != ' ')
			{
				SoundManager.SharedInstance.RandomCharacterSoundEffect();
			}

			//espera a que retorne tantos segundos
			//segundos solo es en tiempo de juego
			//y real, es en segundos del mundo real
			yield return new WaitForSeconds(1 / characterPerSecond);
		}
		isWriting = false;
	}

	public void HandleUpdate()
	{
		timeSinceLastClick += Time.deltaTime;		

		if (Input.GetAxisRaw("Submit")!=0 && !isWriting)
		{
			if (timeSinceLastClick >= timeBetweenClicks )
			{
				timeSinceLastClick = 0;

				currentLine++;
				if (currentLine < currentDialog.Lines.Count)
				{
					StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
				}
				else
				{
					IsBeingShown = false;
					currentLine = 0;
					dialogBox.SetActive(false);
					//para los npc
					OnDialogClose?.Invoke();
					//para el game manager y poder seguir caminando
					OnDialogFinish?.Invoke();
				}
			}
		}
	}
}