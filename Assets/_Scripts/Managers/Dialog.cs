using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialog
{
	[SerializeField]
	List<string> lines;

	//para que se configuren solo desde el editor
	public List<string> Lines => lines;

}
