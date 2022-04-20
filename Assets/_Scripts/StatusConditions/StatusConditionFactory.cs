using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//acrónimos
public enum StatusConditionID
{
    none, brn, frz, par, psn, slp, conf
}

public class StatusConditionFactory
{
    public static void InitFactory()
	{
        //copiamos las claves, uso var para que sea más fácil
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
                    Description="Hace que el pokemon sufra daño en cada turno",
                    StartMessage="ha sido envenenado",
                    OnFinishTurn=PoisonEffect
                }
            },
            {
                StatusConditionID.brn,
                new StatusCondition()
                {
                    Name="Burn",
                    Description="Hace que el pokemon sufra daño en cada turno",
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
                    Description="Hace que el pokemon esté congelado pero se puede curar aleatoriamente durante un turno",
                    StartMessage="ha sido congelado",
                    OnStartTurn= FrozenEffect
                }
            },
            {
                StatusConditionID.slp,
                new StatusCondition()
                {
                    Name="Sleep",
                    Description="Hace que el pokemon duerma un número fijo de turnos",
                    StartMessage="se ha dormido",
                    //son como las de arriba pero se define con parámetro y lo que hace
                    //es una función definida como lambda
                    OnApplyStatusCondition= (Pokemon pokemon) =>
					{
                        pokemon.StatusNumTurns = Random.Range(1,4);
                        Debug.Log($"El pokemon dormirá durante {pokemon.StatusNumTurns}");                        
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
            /*ESTADOS VOLÁTILES*/
            ,
            {
                StatusConditionID.conf,
                new StatusCondition()
                {
                    Name="Confuse",
                    Description="Hace que el pokemon esté confundido y pueda atacarse así mismo",
                    StartMessage="ha sido confundido",
                    //son como las de arriba pero se define con parámetro y lo que hace
                    //es una función definida como lambda
                    OnApplyStatusCondition= (Pokemon pokemon) =>
                    {
                        pokemon.VolatileStatusNumTurns = Random.Range(1,6);
                        Debug.Log($"El pokemon está confundido durante {pokemon.VolatileStatusNumTurns}");
                    },
                    OnStartTurn = (Pokemon pokemon) =>
                    {
                        if(pokemon.VolatileStatusNumTurns<=0)
                        {
                            //se cura
                            pokemon.CureVolatileStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está confundido!");
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

                        //se hace daño así mismo y no puede atacar
                        pokemon.UpdateHP(pokemon.MaxHP/6);
                        pokemon.StatusChangeMessages.Enqueue($"¡Tan confuso que se hiere a sí mismo!");
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
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está paralizado y no puede moverse");
            return false;
		}
        return true;
	}
    static bool FrozenEffect(Pokemon pokemon)
    {
        //probabilidad de estar paralizado de 25%
        if (Random.Range(0, 100) < 25)
        {
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está congelado");
            pokemon.CureStatusCondition();
            return true;
        }

        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está congelado y no puede atacar");
        return false;
    }
}

