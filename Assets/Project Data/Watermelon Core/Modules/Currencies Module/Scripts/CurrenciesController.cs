using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CurrenciesController : MonoBehaviour
    {
        private static CurrenciesController currenciesController;

        [SerializeField] CurrenciesDatabase currenciesDatabase;
        public CurrenciesDatabase CurrenciesDatabase => currenciesDatabase;

        private static Currency[] currencies;
        public static Currency[] Currencies => currencies;

        private static Dictionary<CurrencyType, int> currenciesLink;

        private static bool isInitialised;
        private static event SimpleCallback onModuleInitialised;

        public virtual void Initialise()
        {
            if (isInitialised) return;

            currenciesController = this;

            // Initialsie database
            currenciesDatabase.Initialise();

            // Store active currencies
            currencies = currenciesDatabase.Currencies;

            // Link currencies by the type
            currenciesLink = new Dictionary<CurrencyType, int>();
            //for (int i = 0; i < currencies.Length; i++)
            //{
            //    if (!currenciesLink.ContainsKey(currencies[i].CurrencyType))
            //    {
            //        currenciesLink.Add(currencies[i].CurrencyType, i);
            //    }
            //    else
            //    {
            //        Debug.LogError(string.Format("[Currency Syste]: Currency with type {0} added to database twice!", currencies[i].CurrencyType));
            //    }

            //    var save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int)currencies[i].CurrencyType);
            //    currencies[i].SetSave(save);
            //}

            for (int i = 0; i < currencies.Length; i++)
            {
                // Kiểm tra nếu loại tiền tệ đã tồn tại trong dictionary
                if (!currenciesLink.ContainsKey(currencies[i].CurrencyType))
                {
                    // Thêm loại tiền tệ vào dictionary
                    currenciesLink.Add(currencies[i].CurrencyType, i);

                    // Log thông tin về loại tiền tệ được thêm thành công
                    Debug.Log($"[Currency System]: Added currency with type: {currencies[i].CurrencyType} at index: {i}");
                }
                else
                {
                    // Log lỗi nếu loại tiền tệ đã tồn tại
                    Debug.LogError($"[Currency System Error]: Currency with type: {currencies[i].CurrencyType} is duplicated in the database! Skipping entry at index: {i}");
                }

                // Lấy dữ liệu được lưu từ SaveController
                var save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int)currencies[i].CurrencyType);

                // Áp dụng save vào đối tượng currency
                currencies[i].SetSave(save);

                // Log thông tin về dữ liệu lưu được áp dụng
                Debug.Log($"[Currency System]: Save object for currency type {currencies[i].CurrencyType} successfully applied.");
            }


            isInitialised = true;

            onModuleInitialised?.Invoke();
            onModuleInitialised = null;

            int currentCoin = Get(CurrencyType.Coins);
            Set(CurrencyType.Coins, ResourceBoost.Instance.coin + currentCoin);
            //Set(CurrencyType.Coins, 10000);
        }

        public static bool HasAmount(CurrencyType currencyType, int amount)
        {
            return currencies[currenciesLink[currencyType]].Amount >= amount;
        }

        public static int Get(CurrencyType currencyType)
        {
            return currencies[currenciesLink[currencyType]].Amount;
        }

        public static Currency GetCurrency(CurrencyType currencyType)
        {
            return currencies[currenciesLink[currencyType]];
        }

        public static void Set(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount = amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Invoke currency change event
            currency.InvokeChangeEvent(0);
        }

        public static void Add(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount += amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Invoke currency change event;
            currency.InvokeChangeEvent(amount);
        }

        public static void Substract(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount -= amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Invoke currency change event
            currency.InvokeChangeEvent(-amount);
        }

        public static void SubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            for(int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged += currencyChange;
            }
        }

        public static void UnsubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged -= currencyChange;
            }
        }

        public static void InvokeOrSubcrtibe(SimpleCallback callback)
        {
            if (isInitialised)
            {
                callback?.Invoke();
            }
            else
            {
                onModuleInitialised += callback;
            }
        }
    }

    public delegate void CurrencyChangeDelegate(Currency currency, int difference);
}