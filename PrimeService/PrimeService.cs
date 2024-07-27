namespace Prime.Services;

public class PrimeService
{
    public PrimeService()
    {

    }

    public bool IsPrime(int candidate)
    {
        if (candidate < 2)
        {
            return false;
        }

        return EratosthenesSieve(candidate);
    }

    private bool EratosthenesSieve(int target)
    {
        var boundary = Math.Sqrt(target);
        for (int divisor = 2; divisor <= boundary; divisor++)
        {
            if (target % divisor == 0)
            {
                return false;
            }
        }
        return true;
    }
}
