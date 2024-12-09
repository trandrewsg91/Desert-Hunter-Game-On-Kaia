using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    public void ChangeToShopAndPlay()
    {
        SceneManager.LoadScene("ShopAndPlay");
    }
}
