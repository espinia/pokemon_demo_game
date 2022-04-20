using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField]
    string trainerName;
    public string TrainerName => trainerName;

    [SerializeField]
    Sprite trainerSprite;
    public Sprite TrainerSprite => trainerSprite;

    //frases a decir
    [SerializeField]
    Dialog dialog,afterLoseDialog;

    //icono de alerta
    [SerializeField]
    GameObject exclamationMessage;

    [SerializeField]
    GameObject fov;

    Character character;

    public bool trainerLostBattle = false;

	private void Awake()
	{
        character = GetComponent<Character>();
	}

    IEnumerator ShowExclamationMark()
	{
        exclamationMessage.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        exclamationMessage.SetActive(false);
    }
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        yield return ShowExclamationMark();

        Vector3 diff = player.transform.position - transform.position;

        //le restamos una unidad al normalizar
        Vector3 moveVector = diff - diff.normalized;

        //lo truncamos a enteros para que se desplace solo unidades completas
        moveVector = new Vector2(Mathf.RoundToInt(moveVector.x), Mathf.RoundToInt(moveVector.y));

        yield return character.MoveTowards(moveVector);

        //un mensaje
        DialogManager.SharedInstance.ShowDialog(dialog, () =>
         {
             // programar inicio de la batalla
             GameManager.SharedInstance.StartTrainerBattle(this);
         });
    }

     
    // Start is called before the first frame update
    void Start()
    {
        SetFovDirection(character.Animator.DefaultDirection);
    }

    public void SetFovDirection(FacingDirection direction)
	{
        float angle = 0;
        //colocamos el ángulo

        if(direction==FacingDirection.Right)
		{
            angle = 90f;
		}
        else if (direction == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (direction == FacingDirection.Left)
        {
            angle = 270;
        }

        //se hace la rotación con el ángulo en un eje dado
        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

	public void Interact(Vector3 source)
	{
        //si aun no ha perdido se puede hacer
        if (!trainerLostBattle)
        {
            //mostramos exclamación
            StartCoroutine(ShowExclamationMark());
        }
        //giramos a la fuente
        character.LookTowards(source);

        if (!trainerLostBattle)
        {
            //un mensaje
            DialogManager.SharedInstance.ShowDialog(dialog, () =>
            {
                // programar inicio de la batalla
                GameManager.SharedInstance.StartTrainerBattle(this);
            });
        }
        else
        {
            //dialogo alternativo
            DialogManager.SharedInstance.ShowDialog(afterLoseDialog);
        }
    }

    public void AfterLostBattle()
	{
        trainerLostBattle = true;
        //desactivamos el FOV
        //ya no arranca por visión
        fov.gameObject.SetActive(false);
	}

    private void Update()
    {
        character.HandleUpdate();
    }
}
