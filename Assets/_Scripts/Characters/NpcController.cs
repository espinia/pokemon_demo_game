using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcState
{
    Idle,Walking,Talking
}

public class NpcController : MonoBehaviour,Interactable
{
    [SerializeField]
    Dialog dialog;

    NpcState state;
    //tiempo en estado idle
    float idleTimer = 0f;
    [SerializeField]
    float idleTime=3.0f;

    [SerializeField]
    List<Vector2> moveDirections;
    int currentDirection;

    //para controlar el movimiento
    Character character;

	private void Awake()
	{
        character = GetComponent<Character>();
	}

    public void Interact(Vector3 source)
    {
        if (state == NpcState.Idle)
        {
            //solo habla si está quieto
            state = NpcState.Talking;

            character.LookTowards(source);

            DialogManager.SharedInstance.ShowDialog(dialog,()=>
                {
                    //regresamos estado
                    state = NpcState.Idle;
                    //contador del timer
                    idleTimer = 0f;
                });
        }
    }

	private void Update()
	{
        if(state==NpcState.Idle)
		{
            idleTimer += Time.deltaTime;
            if(idleTimer>idleTime)
			{
                idleTimer = 0f;
                StartCoroutine(Walk());
            }            
		}
        //comunica que se está moviendo para animar
        character.HandleUpdate();
	}

    IEnumerator Walk()
    {
        state = NpcState.Walking;
        Vector2 direction;
        Vector3 oldPosition = transform.position;

        if (moveDirections.Count > 0)
        {
            direction = moveDirections[currentDirection];            
        }
        else
        {
            //prueba aleatoria
            direction=new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
        }

        yield return character.MoveTowards(direction);

        //después de ejecutar se movió, por predefinido?
        if (moveDirections.Count > 0 && transform.position != oldPosition)
        {
            //incrementamos la dirección
            currentDirection = (currentDirection + 1) % moveDirections.Count;
        }

        state = NpcState.Idle;
    }
}
