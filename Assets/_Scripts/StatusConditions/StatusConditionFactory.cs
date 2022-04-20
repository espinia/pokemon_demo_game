using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//acr�nimos
public enum StatusConditionID
{
    none, brn, frz, par, psn, slp, conf
}

public class StatusConditionFactory
{
    public static void InitFactory()
	{
        //copiamos las claves, uso var para que sea m�s f�cil
        foreach(var condition in StatusConditions)
		{
            var id = condition.Key;
            var statusCondition = condition.Value;
            statusCondition.Id = id;
		}

	}
    public static Dictionary<StatusConditionID, StatusCondition> StatusConditions { get; set; } =
        new Dictionary<StatusConditionID, StatusCondition>()
        {   
            {
                StatusConditionID.psn,
                new StatusCondition()
                {                    
                    Name="Poison",
                    Description="Hace que el pokemon sufra da�o en cada turno",
                    StartMessage="ha sido envenenado",
                    OnFinishTurn=PoisonEffect
                }
            },
            {
                StatusConditionID.brn,
                new StatusCondition()
                {
                    Name="Burn",
                    Description="Hace que el pokemon sufra da�o en cada turno",
                    StartMessage="ha sido quemado",
                    OnFinishTurn=BurnEffect
                }
            },
            {
                StatusConditionID.par,
                new StatusCondition()
                {
                    Name="Paralyzed",
                    Description="Hace que el pokemon pueda estar paralizado en el turno",
                    StartMessage="ha sido paralizado",
                    OnStartTurn= ParalyzedEffect
                }
            },
            {
                StatusConditionID.frz,
                new StatusCondition()
                {
                    Name="Frozen",
                    Description="Hace que el pokemon est� congelado pero se puede curar aleatoriamente durante un turno",
                    StartMessage="ha sido congelado",
                    OnStartTurn= FrozenEffect
                }
            },
            {
                StatusConditionID.slp,
                new StatusCondition()
                {
                    Name="Sleep",
                    Description="Hace que el pokemon duerma un n�mero fijo de turnos",
                    StartMessage="se ha dormido",
                    //son como las de arriba pero se define con par�metro y lo que hace
                    //es una funci�n definida como lambda
                    OnApplyStatusCondition= (Pokemon pokemon) =>
					{
                        pokemon.StatusNumTurns = Random.Range(1,4);
                        Debug.Log($"El pokemon dormir� durante {pokemon.StatusNumTurns}");                        
					},
                    OnStartTurn = (Pokemon pokemon) =>
					{
                        if(pokemon.StatusNumTurns<=0)
						{
                            //se cura
                            pokemon.CureStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ha despertado!");
                            return true;
                        }

                        pokemon.StatusNumTurns--;
                        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue dormido");
                        return false;
					}
                }
            }
            /*ESTADOS VOL�TILES*/
            ,
            {
                StatusConditionID.conf,
                new StatusCondition()
                {
                    Name="Confuse",
                    Description="Hace que el pokemon est� confundido y pueda atacarse as� mismo",
                    StartMessage="ha sido confundido",
                    //son como las de arriba pero se define con par�metro y lo que hace
                    //es una funci�n definida como lambda
                    OnApplyStatusCondition= (Pokemon pokemon) =>
                    {
                        pokemon.VolatileStatusNumTurns = Random.Range(1,6);
                        Debug.Log($"El pokemon est� confundido durante {pokemon.VolatileStatusNumTurns}");
                    },
                    OnStartTurn = (Pokemon pokemon) =>
                    {
                        if(pokemon.VolatileStatusNumTurns<=0)
                        {
                            //se cura
                            pokemon.CureVolatileStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no est� confundido!");
                            return true;
                        }

                        pokemon.VolatileStatusNumTurns--;
                        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue confundido");

                        //probabilidad de 50%
                        if(Random.Range(0,2)==0)
                        {
                            //se puede atacar normalmente
                            return true;
                        }

                        //se hace da�o as� mismo y no puede atacar
                        pokemon.UpdateHP(pokemon.MaxHP/6);
                        pokemon.StatusChangeMessages.Enqueue($"�Tan confuso que se hiere a s� mismo!");
                        return false;
                    }
                }
            }
        };

    static void PoisonEffect(Pokemon pokemon)
	{
        //actualizamos el hp
        pokemon.UpdateHP(Mathf.CeilToInt((float)pokemon.MaxHP/8));
        //mensaje para la interfaz
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos del veneno");
	}

    static void BurnEffect(Pokemon pokemon)
    {
        //actualizamos el hp
        pokemon.UpdateHP(Mathf.CeilToInt((float)pokemon.MaxHP / 15));
        //mensaje para la interfaz
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos de la quemadura");
    }

    static bool ParalyzedEffect(Pokemon pokemon)
	{
        //probabilidad de estar paralizado de 25%
        if(Random.Range(0,100)<25)
		{
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} est� paralizado y no puede moverse");
            return false;
		}
        return true;
	}
    static bool FrozenEffect(Pokemon pokemon)
    {
        //probabilidad de estar paralizado de 25%
        if (Random.Range(0, 100) < 25)
        {
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no est� congelado");
            pokemon.CureStatusCondition();
            return true;
        }

        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} est� congelado y no puede atacar");
        return false;
    }
}

