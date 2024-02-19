using System.Collections;
using System.Collections.Generic;
using KulibinSpace.AudioDepot;
using UnityEngine;

public class PlayMusic : MonoBehaviour {

    void Start() {
        GetComponent<PlayMinstrel>().Play();
    }

}
