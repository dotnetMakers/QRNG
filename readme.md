# dotnetMakers' Quantum Random Number Generator

This is a simple project you can build on your desktop with a breadboard and a few components that uses the quantum effects of zener breakdown to generate randome numbers.  A more detailed explanation [is available on YouTube](https://youtu.be/4R5xnTAvBgc).

### Profiling results

Running each operation 100 times and taking the mean:

#### Meadow F7 Module (ProjectLab 4.a)

```
ADC Queries take: 0.0ms
1 byte gen takes: 4.0ms
1 uint gen takes: 10.0ms
1024 bytes takes: 28.0ms
```

76800 bytes takes about 4 minutes;

#### Raspberry Pi Zero 2W (YoshiPi v1b)

This is significantly slower than the Meadow, likely due to the ADC being on the I2C bus and not in the processor.

```
[application] ADC Queries take: 0.000ms
[application] 1 byte gen takes: 6.000ms
[application] 1 uint gen takes: 7.000ms
[application] 1024 bytes takes: 174.000ms
```

76800 bytes takes about 25 minutes;
