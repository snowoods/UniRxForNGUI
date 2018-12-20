using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
using UniRx.Triggers;

public class ButtonRxTest : ObservableUpdateTrigger
{
	[SerializeField] GameObject button;
	[SerializeField] UIInput input;

	void Start () 
	{
		button.OnClickAsObservable()
			.Take(1)
			.Subscribe(_ => Debug.Log("clicked"))
			.AddTo(this);

		input.OnChangeAsObservable()
			.Dump("input")
			.AddTo(this);
		
		// UpdateAsObservable()
		// 	.Dump("update");

		// Observable.EveryUpdate()
		// 	.Dump("everyupdate")
		// 	.AddTo(this);

		// button.OnDoubleClickAsObservable()
		// 	.Subscribe(_ => Debug.Log("double clicked"))
		// 	.AddTo(this);
	}
}
