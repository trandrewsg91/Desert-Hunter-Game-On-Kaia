using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Thirdweb;
using UnityEngine.UI;
using TMPro;
using System;


public class ShopAndPlayManager : MonoBehaviour
{
    public string Address { get; private set; }

    private string receiverAddress = "0xA24d7ECD79B25CE6C66f1Db9e06b66Bd11632E00";

    public Button coin100Button;
    public Button coin200Button;
    public Button coin300Button;

    public Button backButton;

    public TMP_Text buyingStatusText;

    public TMP_Text totalCoinBoughtText;

    private string notEnoughToken = "Not Enough KAIA";

    private void Start()
    {
        ResourceBoost.Instance.coin = 0;
    }

    private void HideAllButtons() {
        coin100Button.interactable = false;
        coin200Button.interactable = false;
        coin300Button.interactable = false;
        backButton.interactable = false;
    }

    private void ShowAllButtons()
    {
        coin100Button.interactable = true;
        coin200Button.interactable = true;
        coin300Button.interactable = true;
        backButton.interactable = true;
    }

    private static float ConvertStringToFloat(string numberStr)
    {
        // Convert the string to a float
        float number = float.Parse(numberStr);

        // Return the float value
        return number;
    }

    public async void SpendTokenToBuyCoin(int indexValue)
    {
        Address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

        HideAllButtons();
        float costValue = 1f;
        if (indexValue == 1) {
            costValue = 1f;
        }
        else if (indexValue == 2) {
            costValue = 2f;
        }
        else if (indexValue == 3)
        {
            costValue = 3f;
        }
        var userBalance = await ThirdwebManager.Instance.SDK.Wallet.GetBalance();
        if (ConvertStringToFloat(userBalance.displayValue) < costValue)
        {
            buyingStatusText.text = notEnoughToken;
        }
        else
        {
            buyingStatusText.text = "Buying...";
            buyingStatusText.gameObject.SetActive(true);
            try
            {
                // Thực hiện chuyển tiền, nếu thành công thì tiếp tục xử lý giao diện
                await ThirdwebManager.Instance.SDK.Wallet.Transfer(receiverAddress, costValue.ToString());

                // Chỉ thực hiện các thay đổi giao diện nếu chuyển tiền thành công
                ShowAllButtons();

                if (indexValue == 1)
                {
                    buyingStatusText.text = "+100 Coins";
                    ResourceBoost.Instance.coin += 100;
                    
                }
                else if (indexValue == 2)
                {
                    buyingStatusText.text = "+200 Coins";
                    ResourceBoost.Instance.coin += 200;
                }
                else if (indexValue == 3)
                {
                    buyingStatusText.text = "+300 Coins";
                    ResourceBoost.Instance.coin += 300;
                }
                totalCoinBoughtText.text = ResourceBoost.Instance.coin.ToString();

            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
                Debug.LogError($"Lỗi khi thực hiện chuyển tiền: {ex.Message}");
                buyingStatusText.text = "Error. Please try again";
                ShowAllButtons();
            }
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Init");
    }
}
