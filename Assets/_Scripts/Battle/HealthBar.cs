using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
    public GameObject healthBar;
	private Image imageBar;
	public Text currentHPText;
	public Text maxHPText;

	/* se fue al manager
	public Color BarColor
	{
		get
		{
			float localScale = healthBar.transform.localScale.x;
			if (localScale < 0.15)
			{
				return clRed;
			}
			else if (localScale < 0.5)
			{
				return clYellow;
			}
			else
			{
				//asi se trabajaría con colores
				//new Color(98f / 255, 178 / 255, 61 / 255);
				return clGreen;
			}
		}
	}*/

	private void Awake()
	{
		imageBar = healthBar.GetComponent<Image>();
	}

	/*	DEMO
	private void Start()
	{
		//si a un vector3 no le ponemos z, la pone en 0
		//escalar barra vida
		healthBar.transform.localScale = new Vector3(0.5f, 1.0f);
	}*/

	/// <summary>
	/// Actualiza barra de vida con el valor normalizado
	/// </summary>
	/// <param name="normalizedValue">Valor de vida normalizado entre 0 y 1</param>
	public void SetHP(Pokemon pokemon)
	{
		float normalizedValue = (float)pokemon.Hp / pokemon.MaxHP;
		healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f);

		if (imageBar == null)
		{
			imageBar = healthBar.GetComponent<Image>();
		} 
		imageBar.color = ColorManager.SharedInstance.GetBarColor(healthBar.transform.localScale.x);

		if(currentHPText!=null)
			currentHPText.text = string.Format("{0}", pokemon.Hp);
		if (maxHPText != null)
			maxHPText.text = string.Format("/{0}", pokemon.MaxHP);
		
	}

	public void UpdateMaxHp(int maxHP)
	{
		maxHPText.text = string.Format("/{0}", maxHP);
	}

	public IEnumerator SetSmoothHp(Pokemon pokemon)
	{
		/* primera versión maual
		//escala actual
		float currentScale = healthBar.transform.localScale.x;
		float updateQuantity = currentScale - normalizedValue;

		//si la diferencia aún no es pequeña
		while((currentScale-normalizedValue)>Mathf.Epsilon)
		{
			//baja poco a poco
			currentScale -= updateQuantity * Time.deltaTime;
			healthBar.transform.localScale = new Vector3(currentScale, 1);

			imageBar.color = BarColor;

			//esperamos al otro frame
			yield return null;
		}

		//nos aseguramos que haya llegado
		healthBar.transform.localScale = new Vector3(normalizedValue, 1);
		imageBar.color = BarColor;
		*/

		float normalizedValue = (float)pokemon.Hp / pokemon.MaxHP;
		Sequence seq = DOTween.Sequence();
		seq.Append( healthBar.transform.DOScaleX(normalizedValue, 1.0f));
		seq.Join(imageBar.DOColor(ColorManager.SharedInstance.GetBarColor(normalizedValue), 1.0f));

		//con la librería para hacer animación del contador
		seq.Join(currentHPText.DOCounter(pokemon.previousHPValue, pokemon.Hp, 1.0f));
		yield return seq.WaitForCompletion();
		
	}
}
