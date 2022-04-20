using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Pokemon
{
    [SerializeField]
    private PokemonBase _base;
    public PokemonBase Base { get => _base; }

    //nivel del pokemon
    [SerializeField]
    private int _level;
    public int Level { get => _level; }

    //vida actual del pokemon
    private int _hp;
    public int Hp
    {
        get => _hp;
        set
        {
            _hp = value;
            //no dejamos que se vaya por encima
            //y pasamos a entero
            _hp = Mathf.FloorToInt(Mathf.Clamp(_hp, 0, MaxHP));
        }
    }

    //experiencia ganada
    private int _experience;
    public int Experience
    {
        get => _experience;
        set => _experience = value;
    }
    public Move CurrentMove { get; set; }

    //lista de ataques del pokemon
    private List<Move> _moves;
    public List<Move> Moves
    {
        get => _moves;
        set => _moves = value;
    }

    //para diccionario <tipo clave, tipo valor>
    //el get es publico, el set es privado
    //estos son genéricos en el nivel que tenga el pokemon
    public Dictionary<Stat, int> Stats { get; private set; }

    //estos serán los de la batalla
    //en el juego original se puede hasta 6 veces hacer una mejora/empeorar
    public Dictionary<Stat, int> StatsBoosted { get; private set; }
    //para guardar el estado alterado
    public StatusCondition StatusCondition { get; set; }
    public int StatusNumTurns { get; set; }

    //estado alterado volátil
    public StatusCondition VolatileStatusCondition { get; set; }
    public int VolatileStatusNumTurns { get; set; }

    //cola de mensajes de estado
    public Queue<string> StatusChangeMessages { get; private set; } = new Queue<string>();

    public event Action OnStatusConditionChanged;

    public bool HasHPChange { get; set; } = false;

    public int previousHPValue;
    //constructor
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;

        InitPokemon();

    }

    public void InitPokemon()
    {
        //inicializamos la experiencia
        _experience = Base.GetNecesaryExpForLevel(_level);

        //creamos la lista de movimientos en memoria
        _moves = new List<Move>();

        //por cada movimiento en la lista de aprendibles
        foreach (LearnableMove lMove in _base.LearnableMoves)
        {
            //el nivel dice que debe aprenderlo
            if (lMove.Level <= _level)
            {
                //instanciamos un nuevo move que es el que lo trae a la vida
                //desde el scriptable que es la base de definición
                //y lo agregamos
                _moves.Add(new Move(lMove.Move));
            }

            if (_moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
            {
                //solo aprendería los 4 primeros
                break;
            }
        }

        CalculateStats();
        //vida inicial del pokemon al máximo de su base
        _hp = MaxHP;                

        ResetBoostings();

        //se inicializan para que estén plenamente identificados
        StatusCondition = null;
        VolatileStatusCondition = null;
    }

    void ResetBoostings()
	{
        StatusChangeMessages  = new Queue<string>();
        
        StatsBoosted = new Dictionary<Stat, int>()
        {
            {Stat.Attack,0 },
            {Stat.Defense,0 },
            {Stat.SpAttack,0 },
            {Stat.SpDefense,0 },
            {Stat.Speed,0 },
            {Stat.Accuracy,0 },
            {Stat.Evasion,0 }
        };
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        //para agregar lo hacemos con add al diccionario
        Stats.Add(Stat.Attack, Mathf.FloorToInt((_base.Attack * _level) / 100.0f) + 2);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((_base.Defense * _level) / 100.0f) + 2);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((_base.SpAttack * _level) / 100.0f) + 2);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((_base.SpDefense * _level) / 100.0f) + 2);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((_base.Speed * _level) / 100.0f) + 2);

        MaxHP = Mathf.FloorToInt((_base.MaxHP * _level) / 20.0f) + 10+Level;
    }

    //ataque base por nivel, son las reglas de pokemon

    //se modifican los getters para obtener del diccionario en vez de calcular siempre
    /*
    public int MaxHP = Mathf.FloorToInt((_base.MaxHP * _level)/20.0f)+10;
    public int Attack => Mathf.FloorToInt((_base.Attack * _level)/100.0f)+2;
    public int Defense => Mathf.FloorToInt((_base.Defense * _level)/100.0f)+2;
    public int SpAttack => Mathf.FloorToInt((_base.SpAttack * _level)/100.0f)+2;
    public int SpDefense => Mathf.FloorToInt((_base.SpDefense * _level)/100.0f)+2;
    public int Speed => Mathf.FloorToInt((_base.Speed * _level)/100.0f)+2;*/

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        //mejora
        int boost = StatsBoosted[stat]; //de -6 a 6

        //1,1.5,2,2.5 ... 4 son los saltos de las mejoras

        //no dejamos que se salga, se asegura al poner los boost
        //float multiplier = Mathf.Min(1.0f + Mathf.Abs(boost) / 2.0f, 4.0f);
        float multiplier =1.0f + Mathf.Abs(boost) / 2.0f;

        if (boost >= 0)
        {
            //mejora            
            //lo dejamos al minimo como int
            statValue = Mathf.FloorToInt(statValue * multiplier);
        }
        else
        {   //empeora
            statValue = Mathf.FloorToInt((float)statValue / multiplier);
        }

        return statValue;
    } 

    public void ApplyBoost(StatBoosting boost)
	{
        Stat stat = boost.stat;
        int value = boost.boost;

        //los boosts
        StatsBoosted[stat] = Mathf.Clamp(StatsBoosted[stat]+value,-6,6);
        Debug.Log($"{stat} se modifica a {StatsBoosted[stat]}");
        if (value > 0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha incrementado su {stat}");
        }
        else if (value<0)
        {
            StatusChangeMessages.Enqueue($"{Base.Name} ha reducido su {stat}");
        }
        else 
        {
            StatusChangeMessages.Enqueue($"{Base.Name} no nota ningún efecto en su {stat}");
        }
    }

    //se puede hacer similar con get público, set privado
    public int MaxHP { get; private set; }
    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);

    public DamageDescription ReceiveDamage(Pokemon attacker,Move move)
	{
        //para el crítico
        float critical = 1f;
        if(Random.Range(0,100f)<8f)
		{
            //8% probabilidad para un crítico
            critical = 2f;
		}

        //multiplicador por tipos, contemplamos ambos
        float type1 = TypeMatrix.GetMultiplierEffectiveness(move.Base.Type, Base.Type1);
        float type2 = TypeMatrix.GetMultiplierEffectiveness(move.Base.Type, Base.Type2);

        //porque no tiene constructor
        DamageDescription damageDesc = new DamageDescription()
        {
            Critical=critical,
            Type=type1*type2,
            Fainted=false
        };

        float attack = (move.Base.IsSpecialMove? attacker.SpAttack : attacker.Attack);
        float defense = (move.Base.IsSpecialMove ? this.SpDefense : this.Defense);

        //modificador del random de la fórmula
        float modifiers=Random.Range(0.85f,1.0f)* type1 * type2 * critical;

        float baseDamage = ((2 * attacker.Level / 5.0f + 2) * move.Base.Power * 
                            (attack / (float) defense)) / 50.0f + 2;

        //int por que la vida es entera
        int totalDamage = Mathf.FloorToInt(baseDamage * modifiers);

        //Debug.Log("damage:"+totalDamage);

        UpdateHP(totalDamage);
        if(Hp<=0)
		{
            damageDesc.Fainted = true;
        }
        
        return damageDesc;
	}

    public void UpdateHP(int damage)
	{
        HasHPChange = true;
        previousHPValue = Hp;
        Hp -= damage;
        if (Hp<0)
        {
            //no dejamos que caiga de 0
            Hp = 0;
        }
    }

    public void SetConditionStatus(StatusConditionID id )
	{
        if (StatusCondition != null)
        {
            return;
        }

        StatusCondition=StatusConditionFactory.StatusConditions[id];
        StatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name}, {StatusCondition.StartMessage}");

        //si alguien está suscrito a la acción se invoca. para actualizar el recuadro
        OnStatusConditionChanged?.Invoke();

    }

    public void CureStatusCondition()
    {
        StatusCondition = null;
        //para quitar la caja del estado en el HUD
        OnStatusConditionChanged?.Invoke();
    }

    public void SetVolatileConditionStatus(StatusConditionID id)
    {
        if (VolatileStatusCondition != null)
        {
            return;
        }

        VolatileStatusCondition = StatusConditionFactory.StatusConditions[id];
        VolatileStatusCondition?.OnApplyStatusCondition?.Invoke(this);
        StatusChangeMessages.Enqueue($"{Base.Name}, {VolatileStatusCondition.StartMessage}");
    }

    public void CureVolatileStatusCondition()
    {
        VolatileStatusCondition = null;
    }

    public Move RandomMove()
	{        
        List<Move> movesWithPP = Moves.Where(m => m.Pp > 0).ToList();

        if (movesWithPP.Count > 0)
        {
            //int randIdx = Random.Range(0, Moves.Count);
            //random es exclusivo con int 
            int randIdx = Random.Range(0, movesWithPP.Count);
            return Moves[randIdx];
        }
        //no hay pps en ningún ataque
        //TODO: implementar combate que hace daño al enemigo y a si mismo
        return null;
	}

    public bool NeedsToLevelUp()
	{
        if (Experience > Base.GetNecesaryExpForLevel(_level + 1))
		{
            int currentMaxHP = MaxHP;
            _level++;
            Hp += (MaxHP - currentMaxHP);
            return true;
		}

        return false;
	}

    public LearnableMove GetLearnableMoveAtCurrentLevel()
	{
        //los filtra por el where, devuelve el primero o Null
        return Base.LearnableMoves.Where(learnableMove => learnableMove.Level == _level).FirstOrDefault(); ;
	}

    public void LearnMove(LearnableMove learnableMove)
	{
        if (Moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
        {
            return;
        }
        
        //se crea una nueva referencia para que no todos
        //apunten al mismo
        Moves.Add(new Move(learnableMove.Move));
	}

    public void OnBattleFinish()
    {
        //se curan estados volátiles
        CureVolatileStatusCondition();

        //se resetean boost
        ResetBoostings();
    }

    public bool OnStartTurn()
	{
        bool canPerformMovement = true;
        //si hay asignado al estado alterado algo
        if(StatusCondition?.OnStartTurn !=null)
		{            
            if(!StatusCondition.OnStartTurn(this))
			{
                //ya no me puedo mover
                canPerformMovement = false;
			}
		}

        //if (canPerformMovement)
        {
            //el volatil?
            if (VolatileStatusCondition?.OnStartTurn != null)
            {
                if(!VolatileStatusCondition.OnStartTurn(this))
				{
                    //ya no me puedo mover
                    canPerformMovement = false;
                }
            }
        }

        //si no se va al caso default
        return canPerformMovement;
	}
    public void OnFinishTurn()
    {
        //el turno ha terminado y se manda a llamar si existe algo
        //como parámetro va el mismo pokemon

        //todos los estados tendrían que tener el OnFinish turn o saldrá exepción
        //se revisa con el operador ?
        StatusCondition?.OnFinishTurn?.Invoke(this);

        //hay lógica para el estado volatil??
        VolatileStatusCondition?.OnFinishTurn?.Invoke(this);
    }
}

public class DamageDescription
{
    public float Critical { get; set; }
    public float Type { get; set; }
    public bool Fainted { get; set; }
}
