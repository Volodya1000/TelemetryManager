namespace TelemetryManager.Core.Utils;

public static class ChecksumCalculator
{
    public static ushort Compute(byte[] data)
    {
        uint checksum = 0;
        int length = data.Length;

        // Суммируем 16-битные слова в порядке big-endian
        for (int i = 0; i < length - 1; i += 2)
        {
            // Формируем слово: первый байт - старший, второй - младший
            ushort word = (ushort)(data[i] << 8 | data[i + 1]);
            checksum += word;

            // Перенос старших битов
            if (checksum > 0xFFFF)
                checksum = (checksum & 0xFFFF) + 1;
        }

        // Обработка нечетного количества байтов
        if (length % 2 == 1)
        {
            // Последний байт считается как старший байт слова (младший=0)
            checksum += (uint)(data[length - 1] << 8);
            if (checksum > 0xFFFF)
                checksum = (checksum & 0xFFFF) + 1;
        }

        // Дополнение до 1
        return (ushort)(~checksum & 0xFFFF);
    }
}
