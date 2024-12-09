using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectWalletManager : MonoBehaviour
{
    // Gọi hàm này để chuyển sang Scene2
    public void ChangeToShopAndPlay()
    {
        SceneManager.LoadScene("ShopAndPlay");
    }
}
