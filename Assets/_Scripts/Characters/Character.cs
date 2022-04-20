using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	CharacterAnimator _animator;
	//velocidad, unidades x segundo como está hecho
	[SerializeField]
	private float speed=2;

	public bool IsMoving { get; private set; }
	public CharacterAnimator Animator => _animator;
	
	private void Awake()
	{
		_animator = GetComponent<CharacterAnimator>();
	}

	public IEnumerator MoveTowards(Vector2 moveVector, Action OnMoveFinish=null)
	{
		//evitamos movimiento en diagonal
		if (moveVector.x != 0)
			moveVector.y = 0;

		//se hace clamp para asegurarnos que está en el rango
		_animator.MoveX = Mathf.Clamp(moveVector.x,-1,1);
		_animator.MoveY = Mathf.Clamp(moveVector.y,-1,1);

		//calculamos la posición a donde hay que ir con un delta
		Vector3 targetPosition = transform.position;
		targetPosition.x += moveVector.x;
		targetPosition.y += moveVector.y;

		if(!IsPathAvailable(targetPosition))
		{
			//rompemos la corrutina
			yield break;
		}

		//marcamos que nos estamos moviendo
		//_animator.IsMoving = true;
		//estaba mejor directo
		IsMoving = true;

		//mientras no llegue al destino
		//medimos la distancia		
		//Mientras sea mayor a la precisión del equipo
		while (Vector3.Distance(transform.position, targetPosition) > Mathf.Epsilon)
		{
			//mover hacia un punto , desde otro y con un delta
			///se  multiplica velocidad por tiempo para obtener espacio
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

			//esperamos al siguiente frame, ya no hay nada más que hacer
			//se espera
			yield return null;

			//hay otros métodos
			//yield return WaitUntil
			//yield return WaitWhile
			//yield return WaitForSeconds
			//yield return EndOfFrame
		}

		//al terminar ya cerca en el punto, lo ponemos exactamente
		transform.position = targetPosition;

		//marcamos que ya no nos movemos
		//estaba mejor directo
		//_animator.IsMoving = false;
		IsMoving = false;

		//disparamos el action si hay
		OnMoveFinish?.Invoke();		
	}

	public void LookTowards(Vector3 target)
	{
		Vector3 diff = target - transform.position;

		// se obtiene como enteros
		int xdiff = Mathf.FloorToInt(diff.x);
		int ydiff = Mathf.FloorToInt(diff.y);

		//evitamos mirar en diagonal, alguna de las componentes debe ser 0
		if (xdiff==0 || ydiff==0)
		{
			//se hace  un clamp par no salirnos de los limites
			_animator.MoveX = Mathf.Clamp(xdiff, -1, 1);
			_animator.MoveY = Mathf.Clamp(ydiff, -1, 1);
		}
		else
		{
			Debug.LogError("ERROR: El personaje no puede moverse o mirar en diagonal");
		}
	}

	//no es necesaria, estaba mejor directo
	public void HandleUpdate()
	{
		_animator.IsMoving = IsMoving;
	}

	private bool IsPathAvailable(Vector3 target)
	{
		Vector3 path = target - transform.position;
		Vector3 direction = path.normalized;
		//iniciamos una unidad después del personaje y a la mitad del punto que llega
		//origen, tamaño de la caja, angulo, dirección o destino a donde llega, distancia
		//regresará un bool, hay sobrecargas

		return !Physics2D.BoxCast(transform.position + direction,//se suma para salir del caracter
						new Vector2(0.3f, 0.3f),
						0,
						direction,
						path.magnitude - 1,//se le resta porque movimos el origen
						GameLayers.SharedInstance.CollisionLayers
						);
	}

	/// <summary>
	/// Comprueba que la zona a la que iremos está disponible
	/// </summary>
	/// <param name="target">Coordenadas destino</param>
	/// <returns>True si target está disponible, false caso contrario</returns>
	private bool IsAvailable(Vector3 target)
	{
		//el centro del circulo es a donde quiero ir
		//radio, la caja mide 1, entonces usamos uno de 0.25 en unidades unity
		//le damos una mascara de layer

		//se le da un or lógico con la otra capa de interactuables
		if (Physics2D.OverlapCircle(target, 0.2f, 
				GameLayers.SharedInstance.SolidObjectsLayer |
				GameLayers.SharedInstance.InteractableLayer) != null)
		{
			//no podemos ir, está ocupada
			return false;
		}

		return true;
	}
}
