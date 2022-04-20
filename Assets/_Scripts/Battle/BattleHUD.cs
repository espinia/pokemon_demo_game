using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    //recordar agregar using UnityEngine.UI;
    public Text pokemonName;
    public Text pokemonLevel;
    public HealthBar healthBar;

    public GameObject statusBox;

    private Pokemon _pokemon;

    public GameObject expBar;

    public void SetPokemonData(Pokemon pokemon)
    {
        //guardamos el pokemon para consultar mas adelante
        _pokemon = pokemon;

        //cambiamos datos en la UI
        pokemonName.text = pokemon.Base.Name;
        SetLevelText();

        //llamamnos la función para que sea el mismo código
        //si se ve mal aqui, actualizar en este punto directamente al inicio de batalla
        //UpdatePokemonData(_pokemon.Hp);

        //tendría que ser así directo para evitar ese tipo de cosas
        healthBar.SetHP(_pokemon);
        //TODO llevarlo para adentro del HPbar
        //pokemonHealth.text = string.Format("{0}/{1}", _pokemon.Hp, _pokemon.MaxHP);

        SetExp();

        //para actualizar edo alterado en interfaz
        SetStatusConditionData();

        //nos suscribimos a evento
        _pokemon.OnStatusConditionChanged += SetStatusConditionData;
    }

    public IEnumerator UpdatePokemonData()
    {

        //healthBar.SetHP(_pokemon.Hp / (float)_pokemon.MaxHP);
        //solo si ha cambiado la vida
        if (_pokemon.HasHPChange)
        {
            //debe ser float
            yield return healthBar.SetSmoothHp(_pokemon);

            //se hace en el healthbar
            //StartCoroutine(DecreaseHealthPoints(oldHPVal));

            _pokemon.HasHPChange = false;
        }
    }
    /*se va al healthbar
    private IEnumerator DecreaseHealthPoints(int oldHPVal)
	{
        while (oldHPVal > _pokemon.Hp)
        {
            oldHPVal--;
            pokemonHealth.text = string.Format("{0}/{1}", oldHPVal, _pokemon.MaxHP);
            //esperamos
            yield return new WaitForSeconds(0.1f);
        }

        pokemonHealth.text = string.Format("{0}/{1}", _pokemon.Hp, _pokemon.MaxHP);
    }*/

    public void SetExp()
    {
        if (expBar == null)
        {
            return;
        }

        expBar.transform.localScale = new Vector3(NormalizedExp(), 1, 1);
    }

    public IEnumerator SetExpSmooth(bool needsToResetBar = false)
    {
        if (expBar == null)
        {
            //rompemos la corrutina
            yield break;
        }

        if (needsToResetBar)
            expBar.transform.localScale = new Vector3(0, 1, 1);
        yield return expBar.transform.DOScaleX(NormalizedExp(), 2.0f).WaitForCompletion();
    }

    float NormalizedExp()
    {
        int currentLevelExp = _pokemon.Base.GetNecesaryExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetNecesaryExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (_pokemon.Experience - currentLevelExp) / (float)(nextLevelExp - currentLevelExp);
        //este va de 0 a 1 pero podría superar más de 1 nivel

        //ese clamp lo deja entre 0 y 1
        return Mathf.Clamp01(normalizedExp);
    }

    public void SetLevelText()
    {
        pokemonLevel.text = $"Lv. {_pokemon.Level}";

    } 

    void SetStatusConditionData()
	{
        if(_pokemon.StatusCondition==null)
		{
            //ocultamos la caja
            statusBox.SetActive(false);
		}
		else
		{
            statusBox.SetActive(true);
            statusBox.GetComponent<Image>().color = ColorManager.StatusConditionColor.GetColorFromStatusCondition(_pokemon.StatusCondition.Id);
            statusBox.GetComponentInChildren<Text>().text = $"{_pokemon.StatusCondition.Id.ToString().ToUpper()}";
        }
	}
}
