using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocaCena : MonoBehaviour {
	public void trocaparacena (int trocaacena) {
		SceneManager.LoadScene (trocaacena);
	}
}
