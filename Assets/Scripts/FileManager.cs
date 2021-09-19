using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System;

public class FileManager
{
    private readonly int ENCRYPTION_CHAR_INDEX = 10;
    public static readonly string FILE_SUFFIX = ".roga";
    private string path, BASE_PATH;
    private readonly byte[][] ENCRYPTION_KEYS = { new byte[] { 0x15, 0x9F, 0x2A, 0x55, 0x42, 0x21, 0xD2, 0x14, 0xE0, 0x78, 0x77, 0xF8, 0x20, 0x07, 0x82, 0xCF },
                                                new byte[] { 0xEC, 0x7B, 0xE4, 0xF3, 0xC8, 0xA1, 0x9C, 0x88, 0xE1, 0x91, 0x79, 0x1F, 0xBA, 0x72, 0xEE, 0x32 },
                                                new byte[] { 0xF9, 0x65, 0xFB, 0x7A, 0x28, 0x01, 0xFA, 0x82, 0x35, 0x6B, 0xB0, 0xEA, 0xA1, 0xC4, 0x65, 0x42 },
                                                new byte[] { 0x57, 0x89, 0x61, 0x97, 0xCD, 0xB0, 0x20, 0x91, 0xB0, 0x68, 0x5E, 0x4D, 0x3F, 0x83, 0x42, 0x69 },
                                                new byte[] { 0x20, 0xCB, 0x21, 0x28, 0x64, 0x0C, 0xFF, 0xC9, 0xEC, 0xC3, 0xB7, 0x32, 0xCF, 0xFD, 0xC9, 0xE7 },
                                                new byte[] { 0xF5, 0x59, 0xFF, 0x90, 0xF3, 0x26, 0x74, 0xBB, 0x94, 0x77, 0xAC, 0x4A, 0x38, 0xDC, 0xC4, 0x15 },
                                                new byte[] { 0xFD, 0x89, 0x76, 0xFC, 0x5C, 0xE2, 0x23, 0xDA, 0x40, 0xEE, 0xD1, 0xFB, 0x4E, 0xAE, 0x14, 0x85 },
                                                new byte[] { 0x07, 0x27, 0x4A, 0xF0, 0x03, 0xC8, 0xBE, 0x6F, 0x57, 0x41, 0x3B, 0xF2, 0x5C, 0x60, 0xAD, 0x5D },
                                                new byte[] { 0x26, 0x5C, 0x5F, 0x8F, 0x99, 0x2B, 0x83, 0x79, 0x45, 0x44, 0xDC, 0x4D, 0x1C, 0xDB, 0x30, 0x96 },
                                                new byte[] { 0x65, 0x9A, 0x2D, 0xE5, 0xDD, 0xC8, 0xC7, 0x1E, 0x45, 0x7F, 0x37, 0xA1, 0x81, 0x95, 0x8C, 0xB3 }};

    public FileManager(string path)
    {
        BASE_PATH = path + "/";
        this.path = BASE_PATH;
    }

    public void writeToFile(List<Setting> settings, string data)
    {
        if (settings != default && settings.Count > 0)
        {
            path = BASE_PATH;
            string fileName = typeof(Setting).ToString().ToLower() + "s";
            List<string> stringList = new List<string>();
            FieldInfo[] fieldInfoList = settings[0].GetType().GetFields();
            foreach (Setting setting in settings)
                foreach (FieldInfo fieldInfo in fieldInfoList)
                    if (fieldInfo.GetValue(setting) is string)
                        stringList.Add(fieldInfo.GetValue(setting).ToString());
            string encryptionString = getEncryptionString(stringList);
            char encryptionCharacter = getEncryptionCharacter(encryptionString);
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            encryptAndWrite(fileName, encryptionCharacter, encryptionValue, data);
        }
    }

    public void writeToFile(PartData partData, string data, string path)
    {
        this.path = path + "/";
        writeToFile(partData, data);
    }

    private void writeToFile(PartData partData, string data)
    {
        if (partData != default)
        {
            string fileName = partData.id;
            List<string> stringList = new List<string>();
            FieldInfo[] fieldInfoList = partData.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fieldInfoList)
                if (fieldInfo.GetValue(partData) is string)
                    stringList.Add(fieldInfo.GetValue(partData).ToString());
            string encryptionString = getEncryptionString(stringList);
            char encryptionCharacter = getEncryptionCharacter(encryptionString);
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            encryptAndWrite(fileName, encryptionCharacter, encryptionValue, data);
        }
    }

    public void writeToFile(PlayerData playerData, string data, string path)
    {
        this.path = path + "/";
        writeToFile(playerData, data);
    }

    private void writeToFile(PlayerData playerData, string data)
    {
        if (playerData != default)
        {
            string fileName = Guid.NewGuid().ToString();
            List<string> stringList = new List<string>();
            PlayerPartData[] humanRobotParts = playerData.humanRobotParts;
            foreach (PlayerPartData playerPartData in humanRobotParts)
                stringList.Add(playerPartData.id);
            string encryptionString = getEncryptionString(stringList);
            char encryptionCharacter = getEncryptionCharacter(encryptionString);
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            encryptAndWrite(fileName, encryptionCharacter, encryptionValue, data);
        }
    }

    public void writeToFile(List<BuildHubStateData> statesData, string data, string path)
    {
        this.path = path + "/";
        writeToFile(statesData, data);
    }

    public void writeToFile(List<BuildHubStateData> statesData, string data)
    {
        if (statesData != default && statesData.Count > 0)
        {
            string fileName = "build_hub";
            char encryptionCharacter = 'i';
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            encryptAndWrite(fileName, encryptionCharacter, encryptionValue, data);
        }
    }

    public void writeToFile(List<FieldStateData> statesData, string data, string path)
    {
        this.path = path + "/";
        writeToFile(statesData, data);
    }

    public void writeToFile(List<FieldStateData> statesData, string data)
    {
        if (statesData != default && statesData.Count > 0)
        {
            string fileName = "field";
            char encryptionCharacter = 'i';
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            encryptAndWrite(fileName, encryptionCharacter, encryptionValue, data);
        }
    }

    public string readFromFile(string fileName)
    {
        if (fileName.Length > 1)
        {
            char encryptionCharacter = fileName[fileName.IndexOf(".") - 1];
            int encryptionValue = getEncryptionValue(encryptionCharacter);
            return decryptAndRead(fileName, encryptionCharacter, encryptionValue);
        }
        return "";
    }

    private string getEncryptionString(List<string> stringList)
    {
        string encryptionString = "";
        foreach (string currentString in stringList)
            encryptionString = (currentString.Length > encryptionString.Length) ? currentString : encryptionString;
        return encryptionString;
    }

    private char getEncryptionCharacter(string encryptionString)
    {
        return (encryptionString.Length > ENCRYPTION_CHAR_INDEX) ? encryptionString[ENCRYPTION_CHAR_INDEX] : encryptionString[encryptionString.Length / 2];
    }

    private int getEncryptionValue(char encryptionCharacter)
    {
        int encryptionValue = (encryptionCharacter - '0') % 10;
        return encryptionValue;
    }

    private void encryptAndWrite(string fileName, char encryptionCharacter, int encryptionValue, string data)
    {
        fileName += encryptionCharacter + FILE_SUFFIX;
        byte[] encryptionKey = ENCRYPTION_KEYS[encryptionValue];
        encryptionKey[0] = Encoding.ASCII.GetBytes(new char[] { encryptionCharacter })[0];
        using (FileStream fileStream = new FileStream(path + fileName, FileMode.OpenOrCreate))
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                byte[] iv = aes.IV;
                fileStream.Write(iv, 0, iv.Length);
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        streamWriter.WriteLine(data);
                }
            }
        }
    }

    private string decryptAndRead(string fileName, char encryptionCharacter, int encryptionValue)
    {
        try
        {
            string decryptedData = "";
            byte[] encryptionKey = ENCRYPTION_KEYS[encryptionValue];
            encryptionKey[0] = Encoding.ASCII.GetBytes(new char[] { encryptionCharacter })[0];
            using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.IV.Length];
                    fileStream.Read(iv, 0, iv.Length);
                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(encryptionKey, iv), CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                            decryptedData = streamReader.ReadToEnd();
                    }
                }
            }
            return decryptedData;
        }
        catch
        {
            return "";
        }
    }
}