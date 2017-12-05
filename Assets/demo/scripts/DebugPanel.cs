using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour {

    public Text gravidade, velocidadeMov, alturaPulo, multiplicadorCorrida, velocidadeRolar, tempoRolar;

    DemoScene _player;

	// Use this for initialization
	void Start () {
        _player = GameObject.FindWithTag("Player").GetComponent<DemoScene>();

    }
	
	// Update is called once per frame
	void Update () {
        SetarValores();

    }

    public void SetarValores()
    {
        if (gravidade.text != "") _player.gravity = float.Parse(gravidade.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        if (velocidadeMov.text != "") _player.runSpeed = float.Parse(velocidadeMov.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        if (alturaPulo.text != "") _player.jumpHeight = float.Parse(alturaPulo.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        if (multiplicadorCorrida.text != "") _player.runMultiplier = float.Parse(multiplicadorCorrida.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        if (velocidadeRolar.text != "") _player.rollingSpeed = float.Parse(velocidadeRolar.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        if (tempoRolar.text != "") _player.rollingTime = float.Parse(tempoRolar.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
    }
}
