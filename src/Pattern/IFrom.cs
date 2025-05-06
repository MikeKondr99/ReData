namespace Pattern;

public interface IFrom<out TSelf, in TFrom>
{
    public static abstract TSelf From(TFrom value);
}