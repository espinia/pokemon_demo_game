using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FacingDirection { Down, Up, Left, Right}

public class CharacterAnimator : MonoBehaviour
{
	public float MoveX, MoveY;
	public bool IsMoving;

	[SerializeField] List<Sprite> walkDownSprites, walkUpSprites, walkLeftSprites, walkRightSprites;
	[SerializeField] FacingDirection defaultDirection=FacingDirection.Down;

	public FacingDirection DefaultDirection => defaultDirection;

	//uno para cada animación
	CustomAnimator walkDownAnim, walkUpAnim, walkLeftAnim, walkRightAnim;
	//el actual
	CustomAnimator currentAnimator;	

	//para mostrarlo
	SpriteRenderer renderer;

	//evitar encadenamientos raros
	private bool wasPreviouslyMoving = false;

	private void Start()
	{
		renderer = GetComponent<SpriteRenderer>();
		//se inicializan los animator
		walkDownAnim = new CustomAnimator(renderer, walkDownSprites);
		walkUpAnim = new CustomAnimator(renderer, walkUpSprites);
		walkLeftAnim = new CustomAnimator(renderer, walkLeftSprites);
		walkRightAnim = new CustomAnimator(renderer, walkRightSprites);

		//ponemos la dirección default
		SetFacingDirection(defaultDirection);

		//el default
		currentAnimator = walkDownAnim;
	}

	private void Update()
	{
		CustomAnimator previousAnimator = currentAnimator;
		//se decide si se cambia a algún animator
		if (MoveX == 1)
		{
			currentAnimator = walkRightAnim;
		}
		else if (MoveX == -1)
		{
			currentAnimator = walkLeftAnim;
		}
		else if (MoveY == 1)
		{
			currentAnimator = walkUpAnim;
		}
		else if (MoveY == -1)
		{
			currentAnimator = walkDownAnim;
		}

		if (previousAnimator != currentAnimator || IsMoving != wasPreviouslyMoving)
		{
			//se reinicia al cambiar de dirección
			//dentro se renderiza el cuadro
			currentAnimator.Start();
		}
		else
		{
			//según yo debe ir así porque ya se renderiza en el otro

			if (IsMoving)
			{
				//renderizamos el cuadro
				currentAnimator.HandleUpdate();
			}
			else
			{
				//mostramos el primero, como si fuera el idle
				renderer.sprite = currentAnimator.AnimFrames[0];
			}
		}
		

		wasPreviouslyMoving = IsMoving;
	}

	public void SetFacingDirection(FacingDirection direction)
	{
		//establecemos las direcciones
		if (direction==FacingDirection.Down)
		{
			MoveY = -1;
		}
		else if (direction == FacingDirection.Up)
		{
			MoveY = 1;
		}
		else if (direction == FacingDirection.Left)
		{
			MoveX = -1;
		}
		else if (direction == FacingDirection.Right)
		{
			MoveX = 1;
		}
	}
}
