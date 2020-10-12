using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ZXing;

namespace MFC_quittance3
{

    /// <summary> 
    /// Обязательные реквизиты (блок «Payee» УФЭБС[1]):
    /// Name - наименование организации;
    /// PersonalAcc - Номер счета получателя платежа;
    /// BankName - Наименование банка получателя платежа;
    /// BIC - БИК;
    /// CorrespAcc - Номер кор./сч. банка получателя платежа.
    /// Дополнительные реквизиты(блок «Payee» УФЭБС[1]): 
    /// PayeeINN - ИНН получателя платежа;
    /// PensAcc - № лицевого счета в системе персонифицированного учета в ПФР - СНИЛС; 
    /// KPP - КПП получателя платежа;
    /// CBC - КБК;
    /// OKTMO - Общероссийский классификатор территорий муниципальных образований (ОКТМО);
    /// PersonalAccount - Лицевой счет бюджетного получателя;
    /// DrawerStatus - Статус составителя платежного документа;
    /// InstNum - Номер учреждения (образовательного, медицинского); 
    /// ClassNum - Номер группы детсада/класса школы; 
    /// SpecFio - ФИО преподавателя, специалиста, оказывающего услугу.   
    /// </summary>
    class PayeeClass //Класс получателя платежа
    {
        //Обязательные реквизиты (блок «Payee» УФЭБС[1]) 
        public string Name;  //наименование организации
        public string PersonalAcc;//Номер счета получателя платежа
        public string BankName;//Наименование банка получателя платежа
        public string BIC;//БИК
        public string CorrespAcc;//Номер кор./сч. банка получателя платежа

        //Дополнительные реквизиты, формат значений которых определяется Альбомом УФЭБС[1] 
        public string PayeeINN;//ИНН получателя платежа
        public string PensAcc;//№ лицевого счета в системе персонифицированного учета в ПФР - СНИЛС  
        public string KPP;//КПП получателя платежа
        public string CBC;//КБК
        public string OKTMO;//Общероссийский классификатор территорий муниципальных образований (ОКТМО)
        public string PersonalAccount; //Лицевой счет бюджетного получателя 
        public string DrawerStatus; //Статус составителя платежного документа
        public string InstNum; //Номер учреждения (образовательного, медицинского) 
        public string ClassNum; //Номер группы детсада/класса школы 
        public string SpecFio; //ФИО преподавателя, специалиста, оказывающего услугу  
    }
    public class PayerClass //Класс плательщика
    {
        public string LastName; //Фамилия плательщика 
        public string FirstName; //Имя плательщика 
        public string MiddleName; //Отчество плательщика 
        public string PayerAddress; //Адрес плательщика 
        public string PayerINN; //ИНН плательщика
        public string PersAcc; //Номер лицевого счета плательщика в организации (в системе учета ПУ) 
        public string BirthDate; //Дата рождения 
        public string Flat; //Номер квартиры 
        public string Phone; //Номер телефона 
        public string PayerIdType; //Вид ДУЛ плательщика 
        public string PayerIdNum; //Номер ДУЛ плательщика 
        public string ChildFio; //Ф.И.О. ребенка/учащегося 
    }
    public class PaymentClass //Класс платежа
    {
        public string Sum; //Сумма платежа, в копейках  
        public string Purpose; //Наименование платежа (назначение)
        public string PaytReason; //Основание налогового платежа
        public string ТaxPeriod; //Налоговый период
        public string TaxPaytKind; //Тип платежа
        public string PaymTerm; //Срок платежа/дата выставления счета 
        public string PaymPeriod; //Период оплаты  
        public string Category; //Вид платежа  
        public string AddAmount; //Сумма страховки/дополнительной услуги/Сумма пени (в копейках)  
    }
    public class DocumentClass //Класс платежного документа
    {
        public string DocDate; //Дата документа 
        public string DocIdx; //Индекс платежного документа 
        public string Contract; //Номер договора 
        public string ServiceName; //Код услуги/название прибора учета 
        public string CounterId; //Номер прибора учета 
        public string CounterVal; //Показание прибора учета  
        public string QuittId; //Номер извещения, начисления, счета 
        public string QuittDate; //Дата извещения/начисления/счета/постановления (для ГИБДД)   
        public string RuleId; //Номер постановления (для ГИБДД)   
        public string ExecId; //Номер исполнительного производства    
        public string RegType; //Код вида платежа (например, для платежей в адрес Росреестра)    
        public string UIN; //Уникальный идентификатор начисления   
    }

    class ResultClass
    {
        private string qrString; //Итоговая строка 
        private Bitmap qrCodeImage;
        private string qrCodeResult;

        public PayeeClass rPayee;
        public PayerClass rPayer;
        public PaymentClass rPayment;
        public DocumentClass rDocument;

        /// <summary>
        /// Функция последовательного построения строки QR кода. ВНИМАНИЕ! Результирующая строка не должна заканчиваться символом "|". Символ следует удалить.
        /// </summary>
        private void ReadPart(object Object)
        {
            //Type myType = Object.GetType();
            FieldInfo[] myField = Object.GetType().GetFields();
            for (int i = 0; i < myField.Length; i++)
            {
                string value = (string)myField[i].GetValue(Object);
                if (value != null)
                {
                    this.qrString += myField[i].Name + "=" + value + "|";
                }
            }
        }
        /// <summary>
        ///  version - Версия стандарта (на данный момент, версия равна «0001»), encode - кодировка: 1 – WIN1251; 2 – UTF8; 3 – КОI8-R. 
        /// </summary>
        public string MakePaymentQRstring(string version, int encode)
        {
            //Служебный блок
            this.qrString = "ST" + version + encode.ToString() + "|";
            //Обязательный блок
            ReadPart(this.rPayee);
            //Доп. блок
            ReadPart(this.rPayer);
            ReadPart(this.rPayment);
            ReadPart(this.rDocument);
            //
            this.qrString = this.qrString.TrimEnd('|');

            return this.qrString;
        }
        /// <summary>
        ///  version - Версия стандарта (на данный момент, версия равна «0001»), encode - кодировка: 1 – WIN1251; 2 – UTF8; 3 – КОI8-R, size - размер QR кода(мм). 
        /// </summary>
        public string encodeQR(string version, int encode, int size)
        {
            MakePaymentQRstring(version, encode);
            this.qrCodeResult = base64_encode(qrCode_encode(this.qrString, size, encode));
            return this.qrCodeResult;
        }
        public Bitmap qrCode_encode(string message, int size, int encode)
        {
            string encodingType = "";
            switch (encode)
            {
                case 1:
                    encodingType = "win1251";
                    break;
                case 2:
                    encodingType = "utf-8";
                    break;
                case 3:
                    encodingType = "koi8-r";
                    break;
            }
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    CharacterSet = encodingType,
                    Width = size,
                    Height = size,
                }
            };
            this.qrCodeImage = writer.Write(message);
            return this.qrCodeImage;
        }
        public string base64_encode(Bitmap QRcode)
        {
            System.IO.MemoryStream ms = new MemoryStream();
            QRcode.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            return Convert.ToBase64String(byteImage); // Get Base64
        }

    }


}
