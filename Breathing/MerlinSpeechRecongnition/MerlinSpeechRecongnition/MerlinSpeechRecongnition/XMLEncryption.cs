using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Linq;

public class XMLEncryption
{
	public void EncryptXML()
	{
        //Using a copy of the intro.xml at the debug folder of the project for testing.
        string xmlData = File.ReadAllText("walk.xml");

        FileStream fs = new FileStream("walk.scn", FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fs);

        string encriptedXML = Crypto.EncryptStringAES(xmlData,"YouW3ntt0F4r");

        sw.Write(encriptedXML);
        sw.Close();
	}

    public void DecryptXML(string fileName)
    {
    }

    public static byte[] ConvertToByteArray(string str, Encoding encoding)
    {
        return encoding.GetBytes(str);
    }

    public static String ToBinary(Byte[] data)
    {
        return string.Join(" ", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
    }

    public static string ToUFT8(string str)
    {
        return "";
    }
}
