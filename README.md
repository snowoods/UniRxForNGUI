# UniRxForNGUI

Must import NGUI first. 
tested by 
	NGUI v2018.3.0. Release date : 2018.12.19
	UniRx v6.2.2. Release date : 2018.09.13


Example scene. (_TEST_/Scenes/Test1.unity)

```
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
	}
```