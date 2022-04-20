using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//el animator anterior de Unity
//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterAnimator))]
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	string trainerName;
	public string TrainerName => trainerName;

	[SerializeField]
	Sprite trainerSprite;
	public Sprite TrainerSprite => trainerSprite;

	//para guardar entrada del jugador
	private Vector2 input;

	//para controlar movimiento
	private Character _character;

	//evento para avisar de la batalla
	public event System.Action OnPokemonEncountered;
	//para avisar de encuentro con entrenador
	public event System.Action<Collider2D> OnEnterTrainersFOV;

	//para detectar lo rápido que presionan las teclas
	float timeSinceLastClick;
	[SerializeField]
	float timeBetweenClicks = 1.0f;

	private void Awake()
	{
		//el de Unity animator
		//_animator = GetComponent<Animator>();
		_character = GetComponent<Character>();
	}

	public void HandleUpdate()
	{
		timeSinceLastClick += Time.deltaTime;

		//debe completar antes un paso
		//estaba mejor directo del animator de character
		if (!_character.IsMoving)
		{
			//input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			//para que nos de numeros enteros
			input.x = Input.GetAxisRaw("Horizontal");
			input.y = Input.GetAxisRaw("Vertical");

			if (input != Vector2.zero)
			{
				//hay movimiento	
				//se configura una acción cuando termine
				StartCoroutine(_character.MoveTowards(input, OnMoveFinish));
			}

		}

		//se comunica
		_character.HandleUpdate();
		
		if(Input.GetAxisRaw("Submit")!=0)
		{
			if (timeSinceLastClick >= timeBetweenClicks)
			{
				timeSinceLastClick = 0;
				Interact();
			}
		}
	}
	
	void OnMoveFinish()
	{
		CheckForPokemon();
		CheckForInTrainesFOV();
	}

	//revisar si intereactuamos con algo
	private void Interact()
	{
		//se toma del animator para que coincida a donde está viendo

		//aqui obtenemos del animator de Unity y se cambio a animator por script
		//Vector3 facingDirection = new Vector3(_animator.GetFloat("MoveX"),_animator.GetFloat("MoveY"));
		Vector3 facingDirection = new Vector3(_character.Animator.MoveX, _character.Animator.MoveY);

		Vector3 interactPosition = transform.position + facingDirection;
		Debug.DrawLine(transform.position, interactPosition, Color.magenta,1.0f);
		Collider2D collider = Physics2D.OverlapCircle(interactPosition, 0.2f, 
			GameLayers.SharedInstance.InteractableLayer);
		if(collider!=null)
		{
			//si la tiene, se revisa con el ?
			//si no llama a interact
			collider.GetComponent<Interactable>()?.Interact(transform.position);
		}
	}

	[SerializeField]
	float verticalOffset = 0.2f;
	void CheckForPokemon()
	{
		//revisamos si estamos en una capa con pokemon
		if (Physics2D.OverlapCircle(transform.position-new Vector3(0, verticalOffset), 0.2f, 
			GameLayers.SharedInstance.PokemonLayer) != null)
		{
			//estamos dentro de la capa con pokemon
			//con 10, 10% probabilidades

			//se podria hacer con un manager
			//se podria tener un item que la modifique
			if(Random.Range(0,100)<10)
			{
				//dejamos de movernos
				_character.Animator.IsMoving = false;
				//Debug.Log("Encuentro pokemon");
				OnPokemonEncountered();
				
			}
			
		}
	}

	void CheckForInTrainesFOV()
	{
		//revisamos si estamos en una capa de los objetos FOV
		Collider2D collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset), 0.2f,
			GameLayers.SharedInstance.FovLayer);
		if ( collider!= null)
		{
			_character.Animator.IsMoving = false;
			//guardamos la referencia al gameObject del fov en que entramos para enviarlo como parámetro
			Debug.Log("En campo de visión de entrenador");
			OnEnterTrainersFOV?.Invoke(collider);

		}
	}
}

