using UnityEngine;

public class GhostController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private int _currentMill = -1; 

    public void FlyToWindmill(int index)
    {
        if (_currentMill == -1)
        {
            if (index == 0) _animator.SetTrigger("flyToRed");
            if (index == 1) _animator.SetTrigger("flyToGreen");
            if (index == 2) _animator.SetTrigger("flyToBlue");
        }
        else
        {
            if (_currentMill == 0 && index == 1) _animator.SetTrigger("RedToGreen");
            if (_currentMill == 0 && index == 2) _animator.SetTrigger("RedToBlue");
            if (_currentMill == 1 && index == 0) _animator.SetTrigger("GreenToRed");
            if (_currentMill == 1 && index == 2) _animator.SetTrigger("GreenToBlue");
            if (_currentMill == 2 && index == 0) _animator.SetTrigger("BlueToRed");
            if (_currentMill == 2 && index == 1) _animator.SetTrigger("BlueToGreen");
        }

        _currentMill = index;
    }

    public void StopBlowing() { }
}