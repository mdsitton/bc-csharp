using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkcs.Tests
{
    /**
	* Exercise the various key stores, making sure we at least get back what we put in!
	* <p>This tests both the PKCS12 key store.</p>
	*/
    [TestFixture]
    public class Pkcs12StoreTest
        : SimpleTest
    {
        private static readonly char[] passwd = "hello world".ToCharArray();

        //
        // pkcs-12 pfx-pdu
        //
        private static readonly byte[] pkcs12 = Base64.Decode(
            "MIACAQMwgAYJKoZIhvcNAQcBoIAkgAQBMAQBgAQBMAQBgAQBBgQBCQQJKoZI"
            + "hvcNAQcBBAGgBAGABAEkBAGABAEEBAEBBAEwBAEEBAEDBAOCAzQEAQQEAQEE"
            + "ATAEAQQEAQMEA4IDMAQBBAQBAQQBBgQBBAQBAQQBCwQBBAQBCwQLKoZIhvcN"
            + "AQwKAQIEAQQEAQEEAaAEAQQEAQMEA4ICpQQBBAQBAQQBMAQBBAQBAwQDggKh"
            + "BAEEBAEBBAEwBAEEBAEBBAEbBAEEBAEBBAEGBAEEBAEBBAEKBAEEBAEKBAoq"
            + "hkiG9w0BDAEDBAEEBAEPBA8wDQQIoagiwNZPJR4CAQEEAQQEAQEEAQQEAQQE"
            + "AQMEA4ICgAQBBAQDggKABIICgEPG0XlhMFyrs4ZWDrvEzl51ICfXd6K2ql2l"
            + "nnxhszUbigtSj6x49VEx4PfOB9fQFeidc5L5An+nKp646NBMIY0UwXGs8BLQ"
            + "au59jtOs987+l7QYIvl6fdGUIuLPhVSnZZDyqD+HQjU/0/ccKFHRif4tlEQq"
            + "aErvZbFeH0pg4ijf1HfgX6gBJGRKdO+msa4qKGnZdHCSLZehyyxvxAmURetg"
            + "yhtEl7RmedTB+4TDs7atekqxkNlD9tfwDUX6sb0IH6qbEA6P/DlVMdaD54Cl"
            + "QDxRzOfIIjklZhv5OMFWtPK0aYPcqyxzLpw1qRAyoTVXpidkj/hpIpgCVBP/"
            + "k5s2+WdGbLgA/4/zSrF6feRCE5llzM2IGxiHVq4oPzzngl3R+Fi5VCPDMcuW"
            + "NRuIOzJA+RNV2NPOE/P3knThDnwiImq+rfxmvZ1u6T06s20RmWK6cxp7fTEw"
            + "lQ9BOsv+mmyV8dr6cYJq4IlRzHdFOyEUBDwfHThyribNKKobO50xh2f93xYj"
            + "Rn5UMOQBJIe3b7OKZt5HOIMrJSZO02IZgvImi9yQWi96PnWa419D1cAsLWvM"
            + "xiN0HqZMbDFfxVM2BZmsxiexLhkHWKwLqfQDzRjJfmVww8fnXpWZhFXKyut9"
            + "gMGEyCNoba4RU3QI/wHKWYaK74qtJpsucuLWBH6UcsHsCry6VZkwRxWwC0lb"
            + "/F3Bm5UKHax5n9JHJ2amQm9zW3WJ0S5stpPObfmg5ArhbPY+pVOsTqBRlop1"
            + "bYJLD/X8Qbs468Bwzej0FhoEU59ZxFrbjLSBsMUYrVrwD83JE9kEazMLVchc"
            + "uCB9WT1g0hxYb7VA0BhOrWhL8F5ZH72RMCYLPI0EAQQEAQEEATEEAQQEAQEE"
            + "AXgEAQQEAQEEATAEAQQEAQEEAVEEAQQEAQEEAQYEAQQEAQEEAQkEAQQEAQkE"
            + "CSqGSIb3DQEJFAQBBAQBAQQBMQQBBAQBAQQBRAQBBAQBAQQBHgQBBAQBAQQB"
            + "QgQBBAQBQgRCAEQAYQB2AGkAZAAgAEcALgAgAEgAbwBvAGsAJwBzACAAVgBl"
            + "AHIAaQBTAGkAZwBuACwAIABJAG4AYwAuACAASQBEBAEEBAEBBAEwBAEEBAEB"
            + "BAEjBAEEBAEBBAEGBAEEBAEBBAEJBAEEBAEJBAkqhkiG9w0BCRUEAQQEAQEE"
            + "ATEEAQQEAQEEARYEAQQEAQEEAQQEAQQEAQEEARQEAQQEARQEFKEcMJ798oZL"
            + "FkH0OnpbUBnrTLgWBAIAAAQCAAAEAgAABAEwBAGABAEGBAEJBAkqhkiG9w0B"
            + "BwYEAaAEAYAEATAEAYAEAQIEAQEEAQAEATAEAYAEAQYEAQkECSqGSIb3DQEH"
            + "AQQBMAQBGwQBBgQBCgQKKoZIhvcNAQwBBgQPMA0ECEE7euvmxxwYAgEBBAGg"
            + "BAGABAEEBAEIBAgQIWDGlBWxnwQBBAQBCAQI2WsMhavhSCcEAQQEAQgECPol"
            + "uHJy9bm/BAEEBAEQBBCiRxtllKXkJS2anKD2q3FHBAEEBAEIBAjKy6BRFysf"
            + "7gQBBAQDggMwBIIDMJWRGu2ZLZild3oz7UBdpBDUVMOA6eSoWiRIfVTo4++l"
            + "RUBm8TpmmGrVkV32PEoLkoV+reqlyWCvqqSjRzi3epQiVwPQ6PV+ccLqxDhV"
            + "pGWDRQ5UttDBC2+u4fUQVZi2Z1i1g2tsk6SzB3MKUCrjoWKvaDUUwXo5k9Vz"
            + "qSLWCLTZCjs3RaY+jg3NbLZYtfMDdYovhCU2jMYV9adJ8MxxmJRz+zPWAJph"
            + "LH8hhfkKG+wJOSszqk9BqGZUa/mnZyzeQSMTEFga1ZB/kt2e8SZFWrTZEBgJ"
            + "oszsL5MObbwMDowNurnZsnS+Mf7xi01LeG0VT1fjd6rn9BzVwuMwhoqyoCNo"
            + "ziUqSUyLEwnGTYYpvXLxzhNiYzW8546KdoEKDkEjhfYsc4XqSjm9NYy/BW/M"
            + "qR+aL92j8hqnkrWkrWyvocUe3mWaiqt7/oOzNZiMTcV2dgjjh9HfnjSHjFGe"
            + "CVhnEWzV7dQIVyc/qvNzOuND8X5IyJ28xb6a/i1vScwGuo/UDgPAaMjGw28f"
            + "siOZBShzde0Kj82y8NilfYLHHeIGRW+N/grUFWhW25mAcBReXDd5JwOqM/eF"
            + "y+4+zBzlO84ws88T1pkSifwtMldglN0APwr4hvUH0swfiqQOWtwyeM4t+bHd"
            + "5buAlXOkSeF5rrLzZ2/Lx+JJmI2pJ/CQx3ej3bxPlx/BmarUGAxaI4le5go4"
            + "KNfs4GV8U+dbEHQz+yDYL+ksYNs1eb+DjI2khbl28jhoeAFKBtu2gGOL5M9M"
            + "CIP/JDOCHimu1YZRuOTAf6WISnG/0Ri3pYZsgQ0i4cXj+WfYwYVjhKX5AcDj"
            + "UKnc4/Cxp+TbbgZqEKRcYVb2q0kOAxkeaNo3WCm+qvUYrwAmKp4nVB+/24rK"
            + "khHiyYJQsETxtOEyvJkVxAS01djY4amuJ4jL0sYnXIhW3Ag93eavbzksGT7W"
            + "Fg1ywpr1x1xpXWIIuVt1k4e+g9fy7Yx7rx0IK1qCSjNwU3QPWbaef1rp0Q/X"
            + "P9IVXYkqo1g/T3SyXqrbZLO+sDjiG4IT3z3fJJqt81sRSVT0QN1ND8l93BG4"
            + "QKzghYw8sZ4FwKPtLky1dDcVTgQBBAQBCAQIK/85VMKWDWYEAQQEAQgECGsO"
            + "Q85CcFwPBAEEBAEIBAhaup6ot9XnQAQBBAQCgaAEgaCeCMadSm5fkLfhErYQ"
            + "DgePZl/rrjP9FQ3VJZ13XrjTSjTRknAbXi0DEu2tvAbmCf0sdoVNuZIZ92W0"
            + "iyaa2/A3RHA2RLPNQz5meTi1RE2N361yR0q181dC3ztkkJ8PLyd74nCtgPUX"
            + "0JlsvLRrdSjPBpBQ14GiM8VjqeIY7EVFy3vte6IbPzodxaviuSc70iXM4Yko"
            + "fQq6oaSjNBFRqkHrBAEEBAEIBAjlIvOf8SnfugQBBAQBCAQIutCF3Jovvl0E"
            + "AQQEAQgECO7jxbucdp/3BAEEBAEIBAidxK3XDLj+BwQBBAQBCAQI3m/HMbd3"
            + "TwwEAQQEA4ICOASCAjgtoCiMfTkjpCRuMhF5gNLRBiNv+xjg6GvZftR12qiJ"
            + "dLeCERI5bvXbh9GD6U+DjTUfhEab/37TbiI7VOFzsI/R137sYy9Tbnu7qkSx"
            + "u0bTvyXSSmio6sMRiWIcakmDbv+TDWR/xgtj7+7C6p+1jfUGXn/RjB3vlyjL"
            + "Q9lFe5F84qkZjnADo66p9gor2a48fgGm/nkABIUeyzFWCiTp9v6FEzuBfeuP"
            + "T9qoKSnCitaXRCru5qekF6L5LJHLNXLtIMSrbO0bS3hZK58FZAUVMaqawesJ"
            + "e/sVfQip9x/aFQ6U3KlSpJkmZK4TAqp9jIfxBC8CclbuwmoXPMomiCH57ykr"
            + "vkFHOGcxRcCxax5HySCwSyPDr8I4+6Kocty61i/1Xr4xJjb+3oyFStIpB24x"
            + "+ALb0Mz6mUa1ls76o+iQv0VM2YFwnx+TC8KC1+O4cNOE/gKeh0ircenVX83h"
            + "GNez8C5Ltg81g6p9HqZPc2pkwsneX2sJ4jMsjDhewV7TyyS3x3Uy3vTpZPek"
            + "VdjYeVIcgAz8VLJOpsIjyHMB57AyT7Yj87hVVy//VODnE1T88tRXZb+D+fCg"
            + "lj2weQ/bZtFzDX0ReiEQP6+yklGah59omeklIy9wctGV1o9GNZnGBSLvQ5NI"
            + "61e9zmQTJD2iDjihvQA/6+edKswCjGRX6rMjRWXT5Jv436l75DVoUj09tgR9"
            + "ytXSathCjQUL9MNXzUMtr7mgEUPETjM/kYBR7CNrsc+gWTWHYaSWuqKVBAEE"
            + "BAEIBAh6slfZ6iqkqwQBBAQBCAQI9McJKl5a+UwEAQQEATgEOBelrmiYMay3"
            + "q0OW2x2a8QQodYqdUs1TCUU4JhfFGFRy+g3yU1cP/9ZSI8gcI4skdPc31cFG"
            + "grP7BAEEBAEIBAhzv/wSV+RBJQQBBAQBCAQI837ImVqqlr4EAQQEAQgECGeU"
            + "gjULLnylBAEEBAEIBAjD3P4hlSBCvQQBBAQBCAQISP/qivIzf50EAQQEAQgE"
            + "CKIDMX9PKxICBAEEBAOCBOgEggTocP5VVT1vWvpAV6koZupKN1btJ3C01dR6"
            + "16g1zJ5FK5xL1PTdA0r6iAwVtgYdxQYnU8tht3bkNXdPJC1BdsC9oTkBg9Nr"
            + "dqlF5cCzXWIezcR3ObjGLpXu49SAHvChH4emT5rytv81MYxZ7bGmlQfp8BNa"
            + "0cMZz05A56LXw//WWDEzZcbKSk4tCsfMXBdGk/ngs7aILZ4FGM620PBPtD92"
            + "pz2Ui/tUZqtQ0WKdLzwga1E/rl02a/x78/OdlVRNeaIYWJWLmLavX98w0PhY"
            + "ha3Tbj/fqq+H3ua6Vv2Ff4VeXazkXpp4tTiqUxhc6aAGiRYckwZaP7OPSbos"
            + "RKFlRLVofSGu1IVSKO+7faxV4IrVaAAzqRwLGkpJZLV7NkzkU1BwgvsAZAI4"
            + "WClPDF228ygbhLwrSN2NK0s+5bKhTCNAR/LCUf3k7uip3ZSe18IwEkUMWiaZ"
            + "ayktcTYn2ZjmfIfV7wIxHgWPkP1DeB+RMS7VZe9zEgJKOA16L+9SNBwJSSs9"
            + "5Sb1+nmhquZmnAltsXMgwOrR12JLIgdfyyqGcNq997U0/KuHybqBVDVu0Fyr"
            + "6O+q5oRmQZq6rju7h+Hb/ZUqRxRoTTSPjGD4Cu9vUqkoNVgwYOT+88FIMYun"
            + "g9eChhio2kwPYwU/9BNGGzh+hAvAKcUpO016mGLImYin+FpQxodJXfpNCFpG"
            + "4v4HhIwKh71OOfL6ocM/518dYwuU4Ds2/JrDhYYFsn+KprLftjrnTBnSsfYS"
            + "t68b+Xr16qv9r6sseEkXbsaNbrGiZAhfHEVBOxQ4lchHrMp4zpduxG4crmpc"
            + "+Jy4SadvS0uaJvADgI03DpsDYffUdriECUqAfOg/Hr7HHyr6Q9XMo1GfIarz"
            + "eUHBgi1Ny0nDTWkdb7I3bIajG+Unr3KfK6dZz5Lb3g5NeclU5zintB1045Jr"
            + "j9fvGGk0/2lG0n17QViBiOzGs2poTlhn7YxmiskwlkRKVafxPZNPxKILpN9s"
            + "YaWGz93qER/pGMJarGJxu8sFi3+yt6FZ4pVPkvKE8JZMEPBBrmH41batS3sw"
            + "sfnJ5CicAkwd8bluQpoc6qQd81HdNpS6u7djaRSDwPtYnZWu/8Hhj4DXisje"
            + "FJBAjQdn2nK4MV7WKVwr+mNcVgOdc5IuOZbRLOfc3Sff6kYVuQFfcCGgAFpd"
            + "nbprF/FnYXR/rghWE7fT1gfzSMNv+z5UjZ5Rtg1S/IQfUM/P7t0UqQ01/w58"
            + "bTlMGihTxHiJ4Qf3o5GUzNmAyryLvID+nOFqxpr5es6kqSN4GPRHsmUIpB9t"
            + "f9Nw952vhsXI9uVkhQap3JvmdAKJaIyDz6Qi7JBZvhxpghVIDh73BQTaAFP9"
            + "5GUcPbYOYJzKaU5MeYEsorGoanSqPDeKDeZxjxJD4xFsqJCoutyssqIxnXUN"
            + "Y3Uojbz26IJOhqIBLaUn6QVFX79buWYjJ5ZkDS7D8kq6DZeqZclt5711AO5U"
            + "uz/eDSrx3d4iVHR+kSeopxFKsrK+KCH3CbBUMIFGX/GE9WPhDWCtjjNKEe8W"
            + "PinQtxvv8MlqGXtv3v7ObJ2BmfIfLD0rh3EB5WuRNKL7Ssxaq14KZGEBvc7G"
            + "Fx7jXLOW6ZV3SH+C3deJGlKM2kVhDdIVjjODvQzD8qw8a/ZKqDO5hGGKUTGD"
            + "Psdd7O/k/Wfn+XdE+YuKIhcEAQQEAQgECJJCZNJdIshRBAEEBAEIBAiGGrlG"
            + "HlKwrAQBBAQBCAQIkdvKinJYjJcEAQQEAUAEQBGiIgN/s1bvPQr+p1aQNh/X"
            + "UQFmay6Vm5HIvPhoNrX86gmMjr6/sg28/WCRtSfyuYjwQkK91n7MwFLOBaU3"
            + "RrsEAQQEAQgECLRqESFR50+zBAEEBAEIBAguqbAEWMTiPwQBBAQBGAQYKzUv"
            + "EetQEAe3cXEGlSsY4a/MNTbzu1WbBAEEBAEIBAiVpOv1dOWZ1AQCAAAEAgAA"
            + "BAIAAAQCAAAEAgAABAIAAAAAAAAAADA1MCEwCQYFKw4DAhoFAAQUvMkeVqe6"
            + "D4UmMHGEQwcb8O7ZwhgEEGiX9DeqtRwQnVi+iY/6Re8AAA==");

        private static readonly byte[] certUTF = Base64.Decode(
            "MIIGVQIBAzCCBg8GCSqGSIb3DQEHAaCCBgAEggX8MIIF+DCCAsUGCSqGSIb3"
            + "DQEHAaCCArYEggKyMIICrjCCAqoGCyqGSIb3DQEMCgEDoIIChTCCAoEGCiqG"
            + "SIb3DQEJFgGgggJxBIICbTCCAmkwggHSoAMCAQICAQcwDQYJKoZIhvcNAQEF"
            + "BQAwOTEPMA0GA1UEBxMGTGV1dmVuMRkwFwYDVQQKExBVdGltYWNvIFN1YiBD"
            + "QSAyMQswCQYDVQQGEwJCRTAeFw05OTEyMzEyMzAwMDBaFw0xOTEyMzEyMzAw"
            + "MDBaMFcxCzAJBgNVBAYTAkJFMQ8wDQYDVQQHEwZIYWFjaHQxEDAOBgNVBAoT"
            + "B1V0aW1hY28xDDAKBgNVBAsMA1ImRDEXMBUGA1UEAxMOR2VlcnQgRGUgUHJp"
            + "bnMwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBANYGIyhTn/p0IA41ElLD"
            + "fZ44PS88AAcDCiOd2DIMLck56ea+5nhI0JLyz1XgPHecc8SLFdl7vSIBA0eb"
            + "tm/A7WIqIp0lcvgoyQ0qsak/dvzs+xw6r2xLCVogku4+/To6UebtfRsukXNI"
            + "ckP5lWV/Ui4l+XvGdmENlEE9/BvOZIvLAgMBAAGjYzBhMBEGA1UdIwQKMAiA"
            + "BlN1YkNBMjAQBgNVHQ4ECQQHVXNlcklEMjAOBgNVHQ8BAf8EBAMCBLAwGQYD"
            + "VR0RBBIwEIEOVXNlcklEMkB1dGkuYmUwDwYDVR0TAQH/BAUwAwEBADANBgkq"
            + "hkiG9w0BAQUFAAOBgQACS7iLLgMV4O5gFdriI7dqX55l7Qn6HiRNxlSH2kCX"
            + "41X82gae4MHFc41qqsC4qm6KZWi1yvTN9XgSBCXTaw1SXGTK7SuNdoYh6ufC"
            + "KuAwy5lsaetyARDksRiOIrNV9j+MRIjJMjPNg+S+ysIHTWZo2NTUuVuZ01D2"
            + "jDtYPhcDFDESMBAGCSqGSIb3DQEJFTEDBAE3MIIDKwYJKoZIhvcNAQcGoIID"
            + "HDCCAxgCAQAwggMRBgkqhkiG9w0BBwEwKAYKKoZIhvcNAQwBAzAaBBS5KxQC"
            + "BMuZ1To+yed2j/TT45td6gICCACAggLYxQS+fu7W2sLQTkslI0EoNxLoH/WO"
            + "L8NgiIgZ5temV3mgC2q0MxjVVq+SCvG89ZSTfptxOaSmYV772irFdzlrtotZ"
            + "wmYk1axuFDYQ1gH0M6i9FWuhOnbk7qHclmOroXqrrbP6g3IsjwztH0+iwBCg"
            + "39f63V0rr8DHiu7zZ2hBkU4/RHEsXLjaCBVNTUSssWhVLisLh2sqBJccPC2E"
            + "1lw4c4WrshGQ+syLGG38ttFgXT1c+xYNpUKqJiJTLVouOH9kK3nH1hPRHKMN"
            + "9CucBdUzibvkcRk1L53F3MfvjhCSNeWEmd9PKN+FtUtzRWQG3L84VGTM37Ws"
            + "YcxaDwDFGcw3u1W8WFsCCkjpZecKN8P2Kp/ai/iugcXY77bYwAwpETDvQFvD"
            + "nnL9oGi03HYdfeiXglC7x7dlojvnpkXDbE0nJiFwhe8Mxpx8GVlGHtP+siXg"
            + "tklubg1eTCSoG9m1rsBJM717ZHXUGf32HNun2dn4vOWGocgBmokZ46KKMb9v"
            + "reT39JTxi8Jlp+2cYb6Qr/oBzudR+D4iAiiVhhhEbJKPNHa61YyxF810fNI2"
            + "GWlNIyN3KcI8XU6WJutm/0H3X8Y+iCSWrJ2exUktj8GiqNQ6Yx0YgEk9HI7W"
            + "t9UVTIsPCgCqrV4SWCOPf6so1JqnpvlPvvNyNxSsAJ7DaJx1+oD2QQfhowk/"
            + "bygkKnRo5Y15ThrTsIyQKsJHTIVy+6K5uFZnlT1DGV3DcNpuk3AY26hrAzWO"
            + "TuWXsULZe7M6h6U2hTT/eplZ/mwHlXdF1VErIuusaCdkSI0doY4/Q223H40L"
            + "BNU3pTezl41PLceSll00WGVr2MunlNeXKnXDJW06lnfs9BmnpV2+Lkfmf30W"
            + "Pn4RKJQc+3D3SV4fCoQLIGrKiZLFfEdGJcMlySr+dJYcEtoZPuo6i/hb5xot"
            + "le63h65ihNtXlEDrNpYSQqnfhjOzk5/+ZvYEcOtDObEwPTAhMAkGBSsOAwIa"
            + "BQAEFMIeDI9l2Da24mtA1fbQIPc6+4dUBBQ8a4lD7j1CA1vRLhdEgPM+5hpD"
            + "RgICCAA=");

        private static readonly byte[] pkcs12noFriendly = Base64.Decode(
            "MIACAQMwgAYJKoZIhvcNAQcBoIAkgASCBAAwgDCABgkqhkiG9w0BBwGggCSA"
            + "BIICvjCCArowggK2BgsqhkiG9w0BDAoBAqCCAqUwggKhMBsGCiqGSIb3DQEM"
            + "AQMwDQQIyJDupEHvySECAQEEggKAupvM7RuZL3G4qNeJM3afElt03TVfynRT"
            + "xUxAZOfx+zekHJTlnEuHJ+a16cOV6dQUgYfyMw1xcq4E+l59rVeMX9V3Zr0K"
            + "tsMN9VYB/9zn62Kw6LQnY0rMlWYf4bt9Ut5ysq0hE5t9FL+NZ5FbFdWBOKsj"
            + "/3oC6eNXOkOFyrY2haPJtD1hVHUosrlC0ffecV0YxPDsReeyx0R4CiYZpAUy"
            + "ZD7rkxL+mSX7zTsShRiga2Q/NEhC1KZpbhO/qbyOgvH0r7CRumSMvijzDgaV"
            + "IGqtrIZ2E2k5kscjcuFTW0x3OZTLAW/UnAh4JXJzC6isbdiWuswbAEBHifUC"
            + "rk2f+bDJKe2gkH67J2K0yDQ3YSSibpjDX/bVfbtfmOoggK9MKQwqEeE0nbYE"
            + "jzInH2OK5jPtmwppjmVA7i3Uk25w2+z7b/suUbft9hPCNjxFvzdbyCcXK4Vv"
            + "xAgEbVWnIkvOQNbyaQi+DEF/4P26GwgJgXuJpMBn0zzsSZSIDLNl8eJHoKp2"
            + "ZXknTi0SZkLaYlBxZlNhFoyXLfvQd6TI2aR5aCVqg1aZMBXyOWfz5t0JTVX8"
            + "HTIcdXKis91iEsLB7vjcxIOASTAjKARr5tRp6OvaVterAyDOn2awYQJLLic5"
            + "pQfditRAlsLkTxlDdu0/QBMXSPptO8g3R+dS7ntvCjXgZZyxpOeKkssS2l5v"
            + "/B2EsfKmYA9hU4aBdW1S9o/PcF1wpVqABd8664TGJ77tCAkbdHe0VJ3Bop2X"
            + "lNxlWeEeD0v0QUZLqkJoMEwi5SUE6HAWjbqGhRuHyey9E+UsdCVnQ8AxXQzL"
            + "2UKOmIrXc6R25GsLPCysXuXPRFBB2Tul0V3re3hPcAAAAAAAADCABgkqhkiG"
            + "9w0BBwaggDCAAgEAMIAGCSqGSIb3DQEHATAbBgoqhkiG9w0BDAEGMA0ECDXn"
            + "UZu6xckzAgEBoIAEggTYQMbzAoGnRVJMbCaJJUYgaARJ4zMfxt2e12H4pX/e"
            + "vnZrR1eKAMck5c2vJoEasr0i2VUcAcK12AntVIEnBwuRBcA2WrZnC28WR+O7"
            + "rLdu9ymG2V3zmk66aTizaB6rcHAzs2lD74n+/zJhZNaDMBfu9LzAdWb/u6Rb"
            + "AThmbw764Zyv9802pET6xrB8ureffgyvQAdlcGHM+yxaOV3ZEtS0cp7i+pb/"
            + "NTiET4jAFoO1tbBrWGJSRrMKvx4ZREppMhG3e/pYglfMFl+1ejbDsOvEUKSt"
            + "H+MVrgDgAv4NsUtNmBu+BIIEAIOCjrBSK3brtV0NZOWsa6hZSSGBhflbEY8s"
            + "U1bDsgZIW4ZaJJvSYEXLmiWSBOgq9VxojMfjowY+zj6ePJJMyI3E7AcFa+on"
            + "zZjeKxkKypER+TtpBeraqUfgf01b6olH8L2i4+1yotCQ0PS+15qRYPK6D+d3"
            + "S+R4veOA6wEsNRijVcB3oQsBCi0FVdf+6MVDvjNzBCZXj0heVi+x0EE106Sz"
            + "B3HaDbB/KNHMPZvvs3J3z2lWLj5w7YZ9eVmrVJKsgG2HRKxtt2IQquRj4BkS"
            + "upFnMTBVgWxXgwXycauC9bgYZurs+DbijqhHfWpUrttDfavsP8aX6+i3gabK"
            + "DH4LQRL7xrTcKkcUHxOTcPHLgDPhi+RevkV+BX9tdajbk4tqw1d+0wOkf1pW"
            + "aTG8fUp0lUpra7EJ0lGy8t/MB3NEk/5tLk9qA2nsKKdNoEdZWiEBE0fMrH1o"
            + "tWJDew3VhspT+Lkor2dLN5ydjcr3wkb76OETPeMxS91onNj5mrAMUBt66vb6"
            + "Gx4CL8FTRNZ/l8Kzngzdv9PmmKPTIXbhYbn3XRGg3od2tC/oVfsqYlGAMgFO"
            + "STt+BZ1BR9Phyi4jsiy8R0seCEDRWYQLbwgwVj0V8Rx9VptqRoCnB4XhGJoJ"
            + "TdAz/MT7KOSxIh2F2FymTJpyImcV6X4Kcj9iY0AZQ4zj712g4yMR6xKGzRu6"
            + "oIBDkFW2bdA3Lb9ePpo5GFtNyA7IbggIko6VOeeOKxaq9nALS2gsZc1yaYtp"
            + "aKL8kB+dVTCXiLgQniO6eMzgonsuwFnG+42XM1vhEpAvFzeJRC0CYzebEK9n"
            + "nGXKCPoqPFuw3gcPMn57NCZJ8MjT/p0wANIEm6AsgqrdFKwTRVJ1ytB/X9Ri"
            + "ysmjMBs9zbFKjU9jVDg1vGBNtb7YnYg9IrYHa3e4yTu2wUJKGP2XWHVgjDR7"
            + "6RtzlO4ljw0kkSMMEDle2ZbGZ6lVXbFwV0wPNPmGA6+XGJRxcddTnrM6R/41"
            + "zqksFLgoNL2BdofMXwv7SzxGyvFhHdRRdBZ5dKj2K9OfXakEcm/asZGu87u8"
            + "y9m7Cckw8ilSNPMdvYiFRoThICx9NiwYl1IIKGcWlb9p6RAx6XNSkY6ZZ6pE"
            + "Vla1E26rbd7is1ssSeqxLXXV9anuG5HDwMIt+CIbD8fZmNTcWMzZRiaFajvR"
            + "gXdyTu/UhVdhiQPF+lrxp4odgF0cXrpcGaKvOtPq04F4ad3O5EkSGucI210Q"
            + "pR/jQs07Yp5xDPzsXAb8naHb84FvK1iONAEjWbfhDxqtH7KGrBbW4KEzJrv3"
            + "B8GLDp+wOAFjGEdGDPkOx3y2L2HuI1XiS9LwL+psCily/A96OiUyRU8yEz4A"
            + "AAAAAAAAAAAEAwAAAAAAAAAAADAtMCEwCQYFKw4DAhoFAAQU1NQjgVRH6Vg3"
            + "tTy3wnQisALy9aYECKiM2gZrLi+fAAA=");

        private static readonly char[] noFriendlyPassword = "sschette12".ToCharArray();

        private static readonly byte[] pkcs12StorageIssue = Base64.Decode(
            "MIIO8QIBAzCCDrEGCSqGSIb3DQEHAaCCDqIEgg6eMIIOmjCCBBMGCSqGSIb3"
            + "DQEHAaCCBAQEggQAMIID/DCCA/gGCyqGSIb3DQEMCgECoIICtjCCArIwHAYK"
            + "KoZIhvcNAQwBAzAOBAgURJ+/5hA2pgICB9AEggKQYZ4POE8clgH9Bjd1XO8m"
            + "sr6NiRBiA08CllHSOn2RzyAgHTa+cKaWrEVVJ9mCd9XveSUCoBF9E1C3jSl0"
            + "XIqLNgYd6mWK9BpeMRImM/5crjy///K4ab9kymzkc5qc0pIpdCQCZ04YmtFP"
            + "B80VCgyaoh2xoxqgjBCIgdSg5XdepdA5nXkG9EsQ1oVUyCykv20lKgKKRseG"
            + "Jo23AX8YUYR7ANqP2gz9lvlX6RBczuoZ62ujopUexiQgt5SZx97sgo3o/b/C"
            + "px17A2L4wLdeAYCMCsZhC2UeaqnZCHSsvnPZfRGiuSEGbV5gHLmXszLDaEdQ"
            + "Bo873GTpKTTzBfRFzNCtYtZRqh2AUsInWZWQUcCeX6Ogwa0wTonkp18/tqsh"
            + "Fj1fVpnsRmjJTTXFxkPtUw5GPJnDAM0t1xqV7kOjN76XnZrMyk2azQ1Mf3Hn"
            + "sGpF+VRGH6JtxbM0Jm5zD9uHcmkSfNR3tP/+vHOB1mkIR9tD2cHvBg7pAlPD"
            + "RfDVWynhS+UBNlQ0SEM/pgR7PytRSUoKc/hhe3N8VerF7VL3BwWfBLlZFYZH"
            + "FvPQg4coxF7+We7nrSQfXvdVBP9Zf0PTdf3pbZelGCPVjOzbzY/o/cB23IwC"
            + "ONxlY8SC1nJDXrPZ5sY51cg/qUqor056YqipRlI6I+FoTMmMDKPAiV1V5ibo"
            + "DNQJkyv/CAbTX4+oFlxgddTwYcPZgd/GoGjiP9yBHHdRISatHwMcM06CzXJS"
            + "s3MhzXWD4aNxvvSpXAngDLdlB7cm4ja2klmMzL7IuxzLXFQFFvYf7IF5I1pC"
            + "YZOmTlJgp0efL9bHjuHFnh0S0lPtlGDOjJ/4YpWvSKDplcPiXhaFVjsUtclE"
            + "oxCC5xppRm8QWS8xggEtMA0GCSsGAQQBgjcRAjEAMBMGCSqGSIb3DQEJFTEG"
            + "BAQBAAAAMGkGCSsGAQQBgjcRATFcHloATQBpAGMAcgBvAHMAbwBmAHQAIABS"
            + "AFMAQQAgAFMAQwBoAGEAbgBuAGUAbAAgAEMAcgB5AHAAdABvAGcAcgBhAHAA"
            + "aABpAGMAIABQAHIAbwB2AGkAZABlAHIwgZsGCSqGSIb3DQEJFDGBjR6BigA3"
            + "AGQAZQBmADUAYgA0ADMANgBjAGEAYgBkADAAMAAyAGQAZAAyADkAMAAzAGIA"
            + "MQA2ADgANgBjADcAOQA0ADgAXwA0ADYAZgAyADYAZgBkADQALQA4ADEAMgBk"
            + "AC0ANABlAGYAYgAtADgAMAA4ADgALQA0ADUAYQBiADkAMQA5ADEAMAA3AGMA"
            + "YzCCCn8GCSqGSIb3DQEHBqCCCnAwggpsAgEAMIIKZQYJKoZIhvcNAQcBMBwG"
            + "CiqGSIb3DQEMAQYwDgQIbr2xdnQ9inMCAgfQgIIKOHg9VKz+jlM+3abi3cp6"
            + "/XMathxDSEJLrxJs6j5DAVX17S4sw1Q/1pptjdMdd8QtTfUB6JpfgJ5Kpn+h"
            + "gZMf6M8wWue0U/RZN0D9w7o+2n+X3ItdEXu80eJVDOm7I2p8qiXtijbMbXRL"
            + "Cup1lgfPM5uv2D63/hmWRXLeG8eySrJnKENngpM559V8TI2JcTUBy1ZP3kcH"
            + "KbcJ/tVPnIIe4qguxfsTmDtAQviGvWUohbt+RGFmtqfgntK7o6b+S8uRSwEs"
            + "fOU/pnVE9M1ugtNJZI/xeGJq6umZWXA/OrAcK7feWUwqRvfivDGQJEoggByd"
            + "4/g92PhK1JGkwlCb1HdfhOOKKChowQ4zVvSOm+uBxARGhk2i5uW9I20I0vSJ"
            + "px42O2VFVJweOchfp+wBtSHBKYP1ZXyXWMvOtULClosSeesbYMAwvyBfpYEz"
            + "3rQt/1iZkqDmEisXk8X1aEKG1KSWaSPyb/+6glWikDm+YdQw3Khu7IZt1l/H"
            + "qWGecccel+R9mT4YjRzHlahUYk4U+RNVasVpH1Kxz2j3CZqL+b3jQOwSAPd/"
            + "hKI+S/pjIpBPfiC4WxORAzGZzY2j+a79B70h1DO1D9jGur3vJDbdmGBNgs6d"
            + "nonE1B527SICcGeXY1MtnZCLOPvySih0AvOekbN9x2CJg+Hp9e7A3Fxni53/"
            + "oMLr9wGRRDki72eXCXW98mU8VJofoWYS1/VBLXGf/f+tJ9J02PpzxleqPH9T"
            + "4mE+YHnZId6cqjCXmwvMr2cMw2clDVfvkbAJRE3eZHzL7IWSO8+giXzzrTsl"
            + "VbMuXVkT4oniTN7TSRsBCT3zVVmCy1QL2hPBD6KsVc+bvLgAHRov84FPrI3f"
            + "kY/oJufT36VE34Eu+QjzULlvVsLE3lhjutOerVIGSP//FM4LE99hp214P0JF"
            + "DgBK+3J+ihmFdW8hUXOt6BU8/MBeiroiJMWo1/f/XcduekG2ZsdGv+GNPzXI"
            + "PyHRpCgAgmck1+qoUPXxHRJuNqv223OZ5MN14X7iLl5OZ+f8IWfxUnZeZ9gj"
            + "HNeceElwZ+YOup1CAi3haD9jxRWhZG4NDfB4IYi4Bc/TAkXE3jCPkYEvIbj9"
            + "ExaU1Ts0+lqOOcwRmBoYjVrz0xbtfR/OWlopyrDHbeL5iQcQCW/loYRapWCZ"
            + "E4ekHknpX9yoAwT355vtTkl0VKXeSZHE8jREhN95aY9zCoLYwbTQDTw7qUR5"
            + "UamabLew0oS0XALtuOrfX4OUOZZUstUsGBle/Pw1TE3Bhe1clhrikp0F+Xgb"
            + "Xx90KqxZX/36RMnCMAD7/q+57rV7WXp2Y5tT0AUgyUMjy1F1X/b1olUfqO1u"
            + "rlWIUTl2znmQ3D9uO3W4ytfgGd5DpKcl2w84MBAT9qGwKuQg/UYKbP4K/+4L"
            + "Y1DWCy3utmohQ28IJtlIUkPL1G7lHX1tfq/VA+bRNTJIhMrNn06ZJpuEJHDs"
            + "/ferdlMFt/d6MrwVivmPVYkb8mSbHSiI8jZOFE44sA974depsDyXafFaSsl0"
            + "bVzqOAu0C/n9dIednU0xxxgDF/djdZ/QhbaDIg2VJf11wx0nw9n76B0+eeyu"
            + "QLaapzxCpQNDVOAM9doBb5F1I5pXQHFQqzTNtLmqDC4x0g8IH7asyk5LCglT"
            + "b1pwMqPJOL2vGWKRLhPzT+9OfSpCmYGKytf593hmGmwIgEO13hQrw31F5TYt"
            + "btkbDr+Q5XilOKEczhEM+Ug7YHU7bxkckOAbxu0YeRp/57GdGLokeLJ0dRlQ"
            + "+V2CfQvWJoVC6PS4PUQtjwgK2p/LU10QsEFwM/S621fGq9zGrv7+FPBATRDb"
            + "k4E9D/WaRylnW11ZTrOlTchQkoHcOh0xztlFxU8jzuIuDrPQQWkoqdl6B+yf"
            + "lykRNJKKxwzFiPl40nLC3nEdIzCEvR4r/9QHiWQxAVSc/wQX+an5vakUmSXS"
            + "oLFjgVdY1jmvdsx2r5BQPuOR8ONGmw/muvVSMaHV85brA4uk0lxn00HD9/a0"
            + "A1LCeFkabNLn9wJT8RaJeOSNmFFllLR70OHaoPSb3GyzHpvd1e6aeaimdyVH"
            + "BQWJ6Ufx+HjbOGuOiN46WyE6Q27dnWxx8qF89dKB4T/J0mEXqueiUjAUnnnR"
            + "Cs4zPaX53hmNBdrZGaLs+xNG8xy+iyBUJIWWfQAQjCjfHYlT9nygiUWIbVQq"
            + "RHkGkAN62jsSNLgHvWVzQPNNsYq0U8TPhyyci/vc8MJytujjptcz8FPqUjg2"
            + "TPv34ef9buErsm4vsdEv/8Z+9aDaNex+O3Lo3N0Aw7M5NcntFBHjFY/nBFNZ"
            + "whH5YA4gQ8PLZ5qshlGvb0DFXHV/9zxnsdPkLwH47ERm5IlEAuoaWtZFxg27"
            + "BjLfwU1Opk+ybDSb5WZVZrs7ljsU85p3Vaf3a//yoyr9ITYj15tTXxSPoct0"
            + "fDUy1I6LjJH/+eZXKA1WSda9mDQlRocvJ0IIIlI4weJpTdm8aHIJ8OngCqOF"
            + "TufcSLDM41+nxEK1LqXeAScVy74kVvvqngj6mIrbylrINZOHheEgTXrUWEc0"
            + "uXS8l1YqY6K6Ru5km2jVyWi/ujrDGb6QGShC09oiDYUuUGy4gwJ3XLVX/dR3"
            + "pmMExohTGiVefFP400wVZaxB9g1BQmjSEZxIaW1U1K6fk8Yni8yWB3/L/PuD"
            + "0+OV+98i1sQGaPe35crIpEc7R2XJdngL0Ol1ZuvCIBfy5DQwGIawTtBnjPdi"
            + "hy//QTt/isdu7C5pGaJDkZFMrfxMibr6c3xXr7wwR75sTzPNmS8mquEdLsmG"
            + "h8gTUnB8/K6V11JtUExMqTimTbUw+j8PggpeBelG36breWJIz1O+dmCTGuLM"
            + "x/sK/i8eiUeRvWjqYpq5DYt4URWg2WlcpcKiUxQp07/NMx0svDC+mlQGwMnJ"
            + "8KOJMW1qr3TGEJ/VVKKVn6sXn/RxA+VPofYzhwZByRX87XmNdPeQKC2DHQsW"
            + "6v83dua5gcnv0cv/smXt7Yr/c12i0fbIaQvj3qjtUCDucjARoBey3eCyG5H6"
            + "5VHSsFnPZ2HCTum+jRSw/ENsu/77XU4BIM2fjAfswp7iIr2Xi4OZWKIj6o6q"
            + "+fNgnOJjemDYHAFK+hWxClrG8b+9Eaf21o4zcHkhCfBlYv4d+xcZOIDsDPwI"
            + "sf+4V+CfoBLALsa2K0pXlPplGom/a8h7CjlyaICbWpEDItqwu7NQwdMRCa7i"
            + "yAyM1sVjXUdcZByS1bjOFSeBe7ygAvEl78vApLxqt8Cw11XSsOtmwssecUN/"
            + "pb7iHE4OMyOgsYx9u7rZ2hMyl42n3c29IwDYMumiNqk9cwCBpQTJAQEv4VzO"
            + "QE5xYDBY9SEozni+4f7B7e2Wj/LOGb3vfNVYGNpDczBFxvr2FXTQla0lNYD/"
            + "aePuC++QW4KvwiGL1Zx4Jo0eoDKWYlYj0qiNlQbWfVw+raaaFnlrq+je0W6P"
            + "+BrKZCncho145y+CFKRLZrN5yl/cDxwsePMVhAIMr1DzVhgBXzA3MB8wBwYF"
            + "Kw4DAhoEFN4Cwj9AtArnRbOIAsRhaaoZlTNJBBTIVPqCrloqLns145CWXjb0"
            + "g141BQ==");

        private static readonly char[] storagePassword = "pass".ToCharArray();

        private static readonly byte[] pkcs12nopass = Base64.Decode(
            "MIIMvgIBAzCCDIQGCSqGSIb3DQEHAaCCDHUEggxxMIIMbTCCCS8GCSqGSIb3"
            + "DQEHBqCCCSAwggkcAgEAMIIJFQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYw"
            + "DgQIfnlhuZRR6/YCAggAgIII6DYgeRwq5n9kzvohZ3JuK+fB+9jZ7Or6EGBA"
            + "GDxtBfHmSNUBWJEV/I8wV1zrKKoW/CaoZfA61pyrVZRd/roaqBx/koTFoh/g"
            + "woyyWTRV9gYTXSVqPQgCH+e2dISAa6UGO+/YOWOOwG2X3t8tS+3FduFQFLt5"
            + "cvUP98zENdm57Aef5pKpBSZDLIAoTASfmqwszWABRh2p/wKOHcCQ9Aj2e2vs"
            + "pls/ntIv81MqPuxHttwX8e+3dKWGFrJRztLpCD2aua8VkSsHFsPxEHkezX4O"
            + "6/VCjMCRFGophTS4dgKKtQIhZ9i/ESlr6sGKgIpyG99ALFpNEhtTKe+T3boE"
            + "sEkhGDquSpu4PGz2m0W5sej1DyFkKX4zIbeMDAb1y3O7aP0F+Llo9QSeGsOA"
            + "aCwND3NUAKBMOHzwdyNQcuCGCqY8j5rrSt99A5FMs3UVW3XU6hRCx7JlzO05"
            + "PNCkcPRSnKSNzBhIR5W0qj4PAZnQTfX+wbtUaDLIqsObX4Muh2l3gl+JmdpO"
            + "53U7ILqN8PAPly1eT+fIrUmlMmFhvo6LbTB7B2K728wsA/5wROlud/mOQz4s"
            + "quS288YsnVc9ExSZKodWa3Pqcdb/cgKNJYDxrR6/eBHOj+0RLK/1yTK9ghj7"
            + "IPYHoEqQbw768WK92RjM+RFGlXASkQhR9y4weWj/388uAWMIbQ+R2Zi4nb31"
            + "knjqRPFThysG1bsRL04/9PgysaasfS9KYOeAlLqp+Ar4gJrof5fytBuY+6wm"
            + "/J8eEdNw7VPV1cz/4rhrd2sfJQwDEN/iZoy8rTwe7wozpwZI0lwH11BBbav+"
            + "1AMfI79jjxhqOeo7uxE2NzUmSd05JYI7a94tcRzGQyGEKpGxYCRamzFW23qb"
            + "vG5Hcqi7Tdd7eTxw4c60l/vQLSo38g6ST5yZrK3URLiAtpioPyjrq2jnVfie"
            + "QLsiAHhpHF01+t+OcKv3UjwdEyBmQ34h9klwiG7iwBFXZaPXFCF2Np1TqFVG"
            + "jjBzmB+hRddEiYwN+XGCKB2Cvgc5ZMQ8LG9jQmEKLmOjuumz1ciAVY2qtl1s"
            + "HYSvfNsIAV/gGzHshOVF19JmGtcQt3pMtupoRh+sh8jY2/x5eIKrj2Jx6HPd"
            + "p/6IPUr54j0xSd6j7gWuXMj/eKp/utMNuBzAhkydnhXYedvTDYIj7SyPPIHa"
            + "qtam8rxTDWn2AOxp7OXTgPmo1GU2zW1OLL1D3MFlS+oaRMfhgNrhW+QP5ay6"
            + "ge4QLijpnSM+p0CbFAOClwzgdJV56bBVV09sDqSBXnG9MeEv5nDaH3I+GpPA"
            + "UgDkaI4zT61kaGgk0uNMf3czy2ycoQzTx0iHDTXSdSqvUC1yFza8UG4AYaKz"
            + "14gtSL7StvZtK0Y8oI084BINI1LgrWyrOLj7vkds4WrKhXm21BtM1GbN/pFh"
            + "XI41h+XoD8KnEPqJ36rAgBo1uHqTNJCC7YikDE/dEvq6MkOx+Nug1YZRHEyi"
            + "3AHry5u1HJHtxT34HXBwRXvnstuFhvU6cjc1WY1dJhu1p82TGnx7OBo/QbcM"
            + "8MRrWmWuU5eW4jWbriGNGYfvZy+tHnGwy0bIeqrsHOG6/JwvfmYYXe64sryH"
            + "5Qo96SZtcTJZaNFwuBY+bFUuOWm8YrT1L7Gl2Muf3pEVtNHLeYARBo1jEAym"
            + "Cb4jw0oodZqbPKdyyzUZu69fdTJiQkMUcKDfHJEGK0Li9SvtdqJLiiJs57Tb"
            + "YfOvn+TIuC40ssJFtmtlGCVH/0vtKLWYeW1NYAMzgI/nlhQ7W6Aroh8sZnqv"
            + "SwxeQmRJaVLxiV6YveTKuVlCbqNVLeEtKYAujgnJtPemGCPbwZpwlBw6V+Dz"
            + "oXveOBcUqATztWJeNv7RbU0Mk7k057+DNxXBIU+eHRGquyHQSBXxBbA+OFuu"
            + "4SPfEAyoYed0HEaoKN9lIsBW1xTROI30MZvaJXvPdLsa8izXGPLnTGmoI+fv"
            + "tJ644HtBCCCr3Reu82ZsTSDMxspZ9aa4ro9Oza+R5eULXDhVXedbhJBYiPPo"
            + "J37El5lRqOgu2SEilhhVQq3ZCugsinCaY9P/RtWG4CFnH1IcIT5+/mivB48I"
            + "2XfH6Xq6ziJdj2/r86mhEnz9sKunNvYPBDGlOvI7xucEf9AiEQoTR1xyFDbW"
            + "ljL4BsJqgsHN02LyUzLwqMstwv+/JH1wUuXSK40Kik/N7+jEFW2C+/N8tN7l"
            + "RPKSLaTjxVuTfdv/BH1dkV4iGFgpQrdWkWgkb+VZP9xE2mLz715eIAg13x6+"
            + "n97tc9Hh375xZJqwr3QyYTXWpsK/vx04RThv8p0qMdqKvf3jVQWwnCnoeBv2"
            + "L4h/uisOLY18qka/Y48ttympG+6DpmzXTwD1LycoG2SOWckCMmJhZK40+zr3"
            + "NVmWf6iJtbLGMxI/kzTqbTaOfXc2MroertyM1rILRSpgnJFxJfai5Enspr9b"
            + "SCwlP718jG2lQsnYlw8CuxoZAiaNy4MmC5Y3qNl3hlcggcHeLodyGkSyRsBg"
            + "cEiKSL7JNvqr0X/nUeW28zVxkmQsWlp3KmST8agf+r+sQvw52fXNLdYznGZV"
            + "rJrwgNOoRj0Z70MwTns3s/tCqDEsy5Sv/5dZW2uQEe7/wvmsP2WLu73Rwplg"
            + "1dwi/Uo9lO9dkEzmoIK5wMPCDINxL1K+0Y79q0tIAEMDgaIxmtRpEh8/TEsA"
            + "UwyEErkDsQqgGviH+ePmawJ/yehYHTRfYUgdUflwApJxRx65pDeSYkiYboMU"
            + "8WSAQY2nh/p9hLlS4zbz9dCK2tzVyRkJgqNy/c4IpiHEx2l1iipW9vENglqx"
            + "dYP4uqD8e3OOLjDQKizWx2t1u7GRwoEVQ3d3QzzOvsRcv7h+6vNsmYqE6phe"
            + "wKFZLctpSn21zkyut444ij4sSr1OG68dEXLY0t0mATfTmXXy5GJBsdK/lLfk"
            + "YTIPYYeDMle9aEicDqaKqkZUuYPnVchGp8UFMJ3M0n48OMDdDvpzBLTxxZeW"
            + "cK5v/m3OEo3jgxy9wXfZdz//J3zXXqvX8LpMy1K9X0uCBTz6ERlawviMQhg1"
            + "1okD5zCCAzYGCSqGSIb3DQEHAaCCAycEggMjMIIDHzCCAxsGCyqGSIb3DQEM"
            + "CgECoIICpjCCAqIwHAYKKoZIhvcNAQwBAzAOBAj3QoojTSbZqgICCAAEggKA"
            + "YOSp5XGdnG1pdm9CfvlAaUSHRCOyNLndoUTqteTZjHTEM9bGwNXAx4/R5H2Q"
            + "PnPm5HB/ynVSXX0uKdW6YlbqUyAdV3eqE4X3Nl+K7ZoXmgAFnMr0tveBhT1b"
            + "7rTi0TN4twjJzBTkKcxT8XKjvpVizUxGo+Ss5Wk8FrWLHAiC5dZvgRemtGcM"
            + "w5S09Pwj+qXpjUhX1pB5/63qWPrjVf+Bfmlz4bWcqogGk0i7eg+OdTeWMrW0"
            + "KR9nD1+/uNEyc4FdGtdIPnM+ax0E+vcco0ExQpTXe0xoX4JW7O71d550Wp89"
            + "hAVPNrJA5eUbSWNsuz+38gjUJ+4XaAEhcA7HZIp6ZyxtzSJUoh7oqpRktoxu"
            + "3cSVqVxIqAEqlNn6j0vbKfW91Od5DI5L+BIxY4xqXS7fdwipj9r6qWA8t9QU"
            + "C2r1A+xXpZ4jEh6inHW9qlfACBBrYf8pSDakSR6yTbaA07LExw0IXz5oiQYt"
            + "s7yx231CZlOH88bBmruLOIZsJjeg/lf63zI7Gg4F85QG3RqEJnY2pinLUTP7"
            + "R62VErFZPc2a85r2dbFH1mSQIj/rT1IKe32zIW8xoHC4VwrPkT3bcLFAu2TH"
            + "5k5zSI/gZUKjPDxb2dwLM4pvsj3gJ9vcFZp6BCuLkZc5rd7CyD8HK9PrBLKd"
            + "H3Yngy4A08W4U3XUtIux95WE+5O/UEmSF7fr2vT//DwZArGUpBPq4Bikb8cv"
            + "0wpOwUv8r0DXveeaPsxdipXlt29Ayywcs6KIidLtCaCX6/0u/XtMsGNFS+ah"
            + "OlumTGBFpbLnagvIf0GKNhbg2lTjflACnxIj8d+QWsnrIU1uC1JRRKCnhpi2"
            + "veeWd1m8GUb3aTFiMCMGCSqGSIb3DQEJFTEWBBS9g+Xmq/8B462FWFfaLWd/"
            + "rlFxOTA7BgkqhkiG9w0BCRQxLh4sAEMAZQByAHQAeQBmAGkAawBhAHQAIAB1"
            + "AHoAeQB0AGsAbwB3AG4AaQBrAGEwMTAhMAkGBSsOAwIaBQAEFKJpUOIj0OtI"
            + "j2CPp38YIFBEqvjsBAi8G+yhJe3A/wICCAA=");

        private static readonly byte[] certsOnly = Base64.Decode(
            "MIICnwIBAzCCApgGCSqGSIb3DQEHAaCCAokEggKFMIICgTCCAn0GCSqGSIb3"
            + "DQEHAaCCAm4EggJqMIICZjCCAmIGCyqGSIb3DQEMCgEDoIICHDCCAhgGCiq"
            + "GSIb3DQEJFgGgggIIBIICBDCCAgAwggFpoAMCAQICBHcheqIwDQYJKoZIhv"
            + "cNAQELBQAwMjENMAsGA1UEChMERGVtbzENMAsGA1UECxMERGVtbzESMBAGA"
            + "1UEAxMJRGVtbyBjZXJ0MCAXDTE5MDgzMTEzMDgzNloYDzIxMDkwNTE5MTMw"
            + "ODM2WjAyMQ0wCwYDVQQKEwREZW1vMQ0wCwYDVQQLEwREZW1vMRIwEAYDVQQ"
            + "DEwlEZW1vIGNlcnQwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAKOVC4"
            + "Qeg0KPAPRB9WcZdvXitiJ+E6rd3czQGNzEFC6FesAllH3PHSWuUZ2YjhiVM"
            + "YJyzwVP1II04iCRaIc65R45oVrHZ2ybWAOda2hBtySjQ2pIQQpoKE7nvL3j"
            + "JcHoCIBJVf3c3xpfh7RucCOGiZDjU9CYPG8yznsazb5+fPF/AgMBAAGjITA"
            + "fMB0GA1UdDgQWBBR/7wUDwa7T0vNzNgjOKdjz2Up9RzANBgkqhkiG9w0BAQ"
            + "sFAAOBgQADzPFsaLhVYD/k9qMueYKi8Ftwijr37niF98cgAHEtq6TGsh3Se"
            + "8gEK3dNJL18vm7NXgGsl8jUWsE9hCF9ar+/cDZ+KrZlZ5PLfifXJJKFqVAh"
            + "sOORef0NRIVcTCoyQTW4pNpNZP9Ul5LJ3iIDjafgJMyEkRbavqdyfSqVTvY"
            + "NpjEzMBkGCSqGSIb3DQEJFDEMHgoAYQBsAGkAYQBzMBYGDGCGSAGG+Watyn"
            + "sBATEGBgRVHSUA");

        private static readonly byte[] sentrixHard = Base64.Decode(
               "MIIK1gIBAzCCCoIGCSqGSIb3DQEHAaCCCnMEggpvMIIKazCCAh8GCSqGSIb3"
             + "DQEHAaCCAhAEggIMMIICCDCCAgQGCyqGSIb3DQEMCgECoIIBEDCCAQwwVwYJ"
             + "KoZIhvcNAQUNMEowKQYJKoZIhvcNAQUMMBwECCwkpMJVacfsAgIH0DAMBggq"
             + "hkiG9w0CCQUAMB0GCWCGSAFlAwQBKgQQB8wdZHiGq1MJtgbAmw6nmASBsMKx"
             + "vNfn6cGwbqScfcHFXS6CVTTIPoEst0ZD6dDHVLThH9Fg+0++lEs7h7XbPAmg"
             + "aBMSpj06LOMnZhzIG10nz6bYYCfqh7wcZ/TUwZL71n+czIbJwBov+XeKgEpH"
             + "Acj4Xjv6DpPr2Z9LJZ9ohJ3gaOn4vEpd7zj6+hm6i4+iiNLgnuoBLYIftzxT"
             + "rm9kscrT93cMkveBBeETe9Exb1vt2g5JF+UVIAs7Qw35OD1dQT5zMYHgMA0G"
             + "CSsGAQQBgjcRAjEAMBMGCSqGSIb3DQEJFTEGBAQBAAAAMFsGCSqGSIb3DQEJ"
             + "FDFOHkwAewAzADUAMgA2ADIAOQBCADIALQAxAEQARgBBAC0ANAA5ADgANQAt"
             + "ADkANgAyAEIALQAyADgAOAA5ADMAMwBEAEMAMAAyADgANQB9MF0GCSsGAQQB"
             + "gjcRATFQHk4ATQBpAGMAcgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUA"
             + "IABLAGUAeQAgAFMAdABvAHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIwgghE"
             + "BgkqhkiG9w0BBwGgggg1BIIIMTCCCC0wggLLBgsqhkiG9w0BDAoBA6CCAqMw"
             + "ggKfBgoqhkiG9w0BCRYBoIICjwSCAoswggKHMIICLaADAgECAhAqxxC4L9yA"
             + "IPEUfBRIgsUUMAoGCCqGSM49BAMCMIGRMQswCQYDVQQGEwJVUzEUMBIGA1UE"
             + "CwwLRW5naW5lZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9PIENvcnBvcmF0aW9u"
             + "MRAwDgYDVQQHDAdSZWRtb25kMRMwEQYDVQQIDApXYXNoaW5ndG9uMSYwJAYD"
             + "VQQDDB1EYXRhIEkvTyBFQyBUZXN0IEludGVybWVkaWF0ZTAeFw0yMDAyMjgy"
             + "MzA3MzJaFw0yNTAyMjgyMzA3MzJaMIGTMQswCQYDVQQGEwJVUzEUMBIGA1UE"
             + "CwwLRW5naW5lZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9PIENvcnBvcmF0aW9u"
             + "MRAwDgYDVQQHDAdSZWRtb25kMRMwEQYDVQQIDApXYXNoaW5ndG9uMSgwJgYD"
             + "VQQDDB9EYXRhIEkvTyBFQyBUZXN0IFNpZ25pbmcgU0hBMjU2MFkwEwYHKoZI"
             + "zj0CAQYIKoZIzj0DAQcDQgAEUgRJSWqivC+PBvi8iGX6AZTbXpe7sBxWROlO"
             + "0czQiQaPQ4KvhJ1JjuE2B3IwxR7QhsuWHEnDUaK9G++Q3e1vnqNjMGEwDwYD"
             + "VR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8EBAMCAe4wHQYDVR0OBBYEFDHinLUw"
             + "lEUi3FXU6HeEcpOs3UIVMB8GA1UdIwQYMBaAFOgH0jlDIZ8zfvQ8dsBdxITd"
             + "ev0GMAoGCCqGSM49BAMCA0gAMEUCICWxWcQ3tPPflX/l2T92rob1LNsWVXch"
             + "mFH3+cFlsxC9AiEA2Dj/whaVyzlIrop0a9J7v5NJCuzZmdd3IeSJrLm5ZxMx"
             + "FTATBgkqhkiG9w0BCRUxBgQEAQAAADCCApcGCyqGSIb3DQEMCgEDoIICcTCC"
             + "Am0GCiqGSIb3DQEJFgGgggJdBIICWTCCAlUwggH6oAMCAQICEGe7tCX/8NRO"
             + "821wo/vWG6IwCgYIKoZIzj0EAwIwgYkxCzAJBgNVBAYTAlVTMRQwEgYDVQQL"
             + "DAtFbmdpbmVlcmluZzEdMBsGA1UECgwURGF0YSBJL08gQ29ycG9yYXRpb24x"
             + "EDAOBgNVBAcMB1JlZG1vbmQxEzARBgNVBAgMCldhc2hpbmd0b24xHjAcBgNV"
             + "BAMMFURhdGEgSS9PIEVDIFRlc3QgUm9vdDAeFw0yMDAyMjgyMzA3MzJaFw0y"
             + "NTAyMjgyMzA3MzJaMIGJMQswCQYDVQQGEwJVUzEUMBIGA1UECwwLRW5naW5l"
             + "ZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9PIENvcnBvcmF0aW9uMRAwDgYDVQQH"
             + "DAdSZWRtb25kMRMwEQYDVQQIDApXYXNoaW5ndG9uMR4wHAYDVQQDDBVEYXRh"
             + "IEkvTyBFQyBUZXN0IFJvb3QwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAAQD"
             + "diFKicUv9eMnuVxtjqPZehcK0WI5G32Zb8SFC9NNgC5XrsL+RuuBE0XtWwih"
             + "vjT+qNj8zPFQWXFvs4FzJbH1o0IwQDAPBgNVHRMBAf8EBTADAQH/MA4GA1Ud"
             + "DwEB/wQEAwIB7jAdBgNVHQ4EFgQU6hTsqwM3Uic8t4OMvD+l2YO1c4cwCgYI"
             + "KoZIzj0EAwIDSQAwRgIhAPMJtaJPuJD17ghhshwBDVrF4Hj1aN+nre7i61L5"
             + "PISrAiEA397jjZDfkiYFihxhCzkh4QHRdxtce06uhrbaZLwIGtIxEzARBgkq"
             + "hkiG9w0BCRQxBB4CAAAwggK/BgsqhkiG9w0BDAoBA6CCApkwggKVBgoqhkiG"
             + "9w0BCRYBoIIChQSCAoEwggJ9MIICI6ADAgECAhBo7ji8qjZ1gybJzknvHrm5"
             + "MAoGCCqGSM49BAMCMIGJMQswCQYDVQQGEwJVUzEUMBIGA1UECwwLRW5naW5l"
             + "ZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9PIENvcnBvcmF0aW9uMRAwDgYDVQQH"
             + "DAdSZWRtb25kMRMwEQYDVQQIDApXYXNoaW5ndG9uMR4wHAYDVQQDDBVEYXRh"
             + "IEkvTyBFQyBUZXN0IFJvb3QwHhcNMjAwMjI4MjMwNzMyWhcNMjUwMjI4MjMw"
             + "NzMyWjCBkTELMAkGA1UEBhMCVVMxFDASBgNVBAsMC0VuZ2luZWVyaW5nMR0w"
             + "GwYDVQQKDBREYXRhIEkvTyBDb3Jwb3JhdGlvbjEQMA4GA1UEBwwHUmVkbW9u"
             + "ZDETMBEGA1UECAwKV2FzaGluZ3RvbjEmMCQGA1UEAwwdRGF0YSBJL08gRUMg"
             + "VGVzdCBJbnRlcm1lZGlhdGUwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAAQO"
             + "PdmrWzvwmWDlkwJ2dfixfVIRo1pZSdJjwNESLJ9VljZecxuYY6xFL+Dg+ihd"
             + "e4qKxEld4/6TuRz7Fja9ScfLo2MwYTAPBgNVHRMBAf8EBTADAQH/MA4GA1Ud"
             + "DwEB/wQEAwIB7jAdBgNVHQ4EFgQU6AfSOUMhnzN+9Dx2wF3EhN16/QYwHwYD"
             + "VR0jBBgwFoAU6hTsqwM3Uic8t4OMvD+l2YO1c4cwCgYIKoZIzj0EAwIDSAAw"
             + "RQIgYx9rf4YGan3nkCQLAE1FjyX1ACWSToTFur9UoPgV3IoCIQDd6Lvuf1IH"
             + "IZPeNbyuvBCynu+eDek8pO5B90BV6ImIuTETMBEGCSqGSIb3DQEJFDEEHgIA"
             + "ADBLMC8wCwYJYIZIAWUDBAIBBCDAU3cgA2uJqlRZ+VamaDDOoqvwwnJKOBUy"
             + "vn/gZKm4FgQU91hwRSdR+ekb4Lt7bjUUxaC/XYYCAgfQ");

        private static readonly byte[] sentrixSoft = Base64.Decode(
               "MIIKgQIBAzCCCj0GCSqGSIb3DQEHAaCCCi4EggoqMIIKJjCCAdoGCSqGSIb3"
             + "DQEHAaCCAcsEggHHMIIBwzCCAb8GCyqGSIb3DQEMCgECoIHMMIHJMBwGCiqG"
             + "SIb3DQEMAQMwDgQItChrbpZXBp8CAgfQBIGoEus4fyZZwKntfPRRch685zDx"
             + "xpdttcCg935o0sLeu9y/iWbtdM0TyhMWS8LBMZgu5Ssh0FwNUJIxCYOGVCXr"
             + "V2ab//ARB7FwX4HgiSAQ5/De98hJ7LWBJ7eom6Bs8qi58c+FIz6zntAF1c4x"
             + "pI80uTYiGgCs8/TDkEM88awQoL1USQ7SNxHrPnIagR1TIL1MHiwZ229jhjt7"
             + "5nzwI+XFgLZ+wc7RCy3MMYHgMA0GCSsGAQQBgjcRAjEAMBMGCSqGSIb3DQEJ"
             + "FTEGBAQBAAAAMFsGCSqGSIb3DQEJFDFOHkwAewAzADUAMgA2ADIAOQBCADIA"
             + "LQAxAEQARgBBAC0ANAA5ADgANQAtADkANgAyAEIALQAyADgAOAA5ADMAMwBE"
             + "AEMAMAAyADgANQB9MF0GCSsGAQQBgjcRATFQHk4ATQBpAGMAcgBvAHMAbwBm"
             + "AHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABvAHIAYQBnAGUA"
             + "IABQAHIAbwB2AGkAZABlAHIwgghEBgkqhkiG9w0BBwGgggg1BIIIMTCCCC0w"
             + "ggLLBgsqhkiG9w0BDAoBA6CCAqMwggKfBgoqhkiG9w0BCRYBoIICjwSCAosw"
             + "ggKHMIICLaADAgECAhAqxxC4L9yAIPEUfBRIgsUUMAoGCCqGSM49BAMCMIGR"
             + "MQswCQYDVQQGEwJVUzEUMBIGA1UECwwLRW5naW5lZXJpbmcxHTAbBgNVBAoM"
             + "FERhdGEgSS9PIENvcnBvcmF0aW9uMRAwDgYDVQQHDAdSZWRtb25kMRMwEQYD"
             + "VQQIDApXYXNoaW5ndG9uMSYwJAYDVQQDDB1EYXRhIEkvTyBFQyBUZXN0IElu"
             + "dGVybWVkaWF0ZTAeFw0yMDAyMjgyMzA3MzJaFw0yNTAyMjgyMzA3MzJaMIGT"
             + "MQswCQYDVQQGEwJVUzEUMBIGA1UECwwLRW5naW5lZXJpbmcxHTAbBgNVBAoM"
             + "FERhdGEgSS9PIENvcnBvcmF0aW9uMRAwDgYDVQQHDAdSZWRtb25kMRMwEQYD"
             + "VQQIDApXYXNoaW5ndG9uMSgwJgYDVQQDDB9EYXRhIEkvTyBFQyBUZXN0IFNp"
             + "Z25pbmcgU0hBMjU2MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEUgRJSWqi"
             + "vC+PBvi8iGX6AZTbXpe7sBxWROlO0czQiQaPQ4KvhJ1JjuE2B3IwxR7QhsuW"
             + "HEnDUaK9G++Q3e1vnqNjMGEwDwYDVR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8E"
             + "BAMCAe4wHQYDVR0OBBYEFDHinLUwlEUi3FXU6HeEcpOs3UIVMB8GA1UdIwQY"
             + "MBaAFOgH0jlDIZ8zfvQ8dsBdxITdev0GMAoGCCqGSM49BAMCA0gAMEUCICWx"
             + "WcQ3tPPflX/l2T92rob1LNsWVXchmFH3+cFlsxC9AiEA2Dj/whaVyzlIrop0"
             + "a9J7v5NJCuzZmdd3IeSJrLm5ZxMxFTATBgkqhkiG9w0BCRUxBgQEAQAAADCC"
             + "ApcGCyqGSIb3DQEMCgEDoIICcTCCAm0GCiqGSIb3DQEJFgGgggJdBIICWTCC"
             + "AlUwggH6oAMCAQICEGe7tCX/8NRO821wo/vWG6IwCgYIKoZIzj0EAwIwgYkx"
             + "CzAJBgNVBAYTAlVTMRQwEgYDVQQLDAtFbmdpbmVlcmluZzEdMBsGA1UECgwU"
             + "RGF0YSBJL08gQ29ycG9yYXRpb24xEDAOBgNVBAcMB1JlZG1vbmQxEzARBgNV"
             + "BAgMCldhc2hpbmd0b24xHjAcBgNVBAMMFURhdGEgSS9PIEVDIFRlc3QgUm9v"
             + "dDAeFw0yMDAyMjgyMzA3MzJaFw0yNTAyMjgyMzA3MzJaMIGJMQswCQYDVQQG"
             + "EwJVUzEUMBIGA1UECwwLRW5naW5lZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9P"
             + "IENvcnBvcmF0aW9uMRAwDgYDVQQHDAdSZWRtb25kMRMwEQYDVQQIDApXYXNo"
             + "aW5ndG9uMR4wHAYDVQQDDBVEYXRhIEkvTyBFQyBUZXN0IFJvb3QwWTATBgcq"
             + "hkjOPQIBBggqhkjOPQMBBwNCAAQDdiFKicUv9eMnuVxtjqPZehcK0WI5G32Z"
             + "b8SFC9NNgC5XrsL+RuuBE0XtWwihvjT+qNj8zPFQWXFvs4FzJbH1o0IwQDAP"
             + "BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIB7jAdBgNVHQ4EFgQU6hTs"
             + "qwM3Uic8t4OMvD+l2YO1c4cwCgYIKoZIzj0EAwIDSQAwRgIhAPMJtaJPuJD1"
             + "7ghhshwBDVrF4Hj1aN+nre7i61L5PISrAiEA397jjZDfkiYFihxhCzkh4QHR"
             + "dxtce06uhrbaZLwIGtIxEzARBgkqhkiG9w0BCRQxBB4CAAAwggK/BgsqhkiG"
             + "9w0BDAoBA6CCApkwggKVBgoqhkiG9w0BCRYBoIIChQSCAoEwggJ9MIICI6AD"
             + "AgECAhBo7ji8qjZ1gybJzknvHrm5MAoGCCqGSM49BAMCMIGJMQswCQYDVQQG"
             + "EwJVUzEUMBIGA1UECwwLRW5naW5lZXJpbmcxHTAbBgNVBAoMFERhdGEgSS9P"
             + "IENvcnBvcmF0aW9uMRAwDgYDVQQHDAdSZWRtb25kMRMwEQYDVQQIDApXYXNo"
             + "aW5ndG9uMR4wHAYDVQQDDBVEYXRhIEkvTyBFQyBUZXN0IFJvb3QwHhcNMjAw"
             + "MjI4MjMwNzMyWhcNMjUwMjI4MjMwNzMyWjCBkTELMAkGA1UEBhMCVVMxFDAS"
             + "BgNVBAsMC0VuZ2luZWVyaW5nMR0wGwYDVQQKDBREYXRhIEkvTyBDb3Jwb3Jh"
             + "dGlvbjEQMA4GA1UEBwwHUmVkbW9uZDETMBEGA1UECAwKV2FzaGluZ3RvbjEm"
             + "MCQGA1UEAwwdRGF0YSBJL08gRUMgVGVzdCBJbnRlcm1lZGlhdGUwWTATBgcq"
             + "hkjOPQIBBggqhkjOPQMBBwNCAAQOPdmrWzvwmWDlkwJ2dfixfVIRo1pZSdJj"
             + "wNESLJ9VljZecxuYY6xFL+Dg+ihde4qKxEld4/6TuRz7Fja9ScfLo2MwYTAP"
             + "BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIB7jAdBgNVHQ4EFgQU6AfS"
             + "OUMhnzN+9Dx2wF3EhN16/QYwHwYDVR0jBBgwFoAU6hTsqwM3Uic8t4OMvD+l"
             + "2YO1c4cwCgYIKoZIzj0EAwIDSAAwRQIgYx9rf4YGan3nkCQLAE1FjyX1ACWS"
             + "ToTFur9UoPgV3IoCIQDd6Lvuf1IHIZPeNbyuvBCynu+eDek8pO5B90BV6ImI"
             + "uTETMBEGCSqGSIb3DQEJFDEEHgIAADA7MB8wBwYFKw4DAhoEFBANsR/ZVCiJ"
             + "TzN4ZU2XvrYRR08dBBSWTwZIoxkxjXzd/mWKRFVbSOKwCAICB9A=");

        private static readonly byte[] sentrix1 = Base64.Decode(
          "MIILRQIBAzCCCvEGCSqGSIb3DQEHAaCCCuIEggreMIIK2jCCAhAGCSqGSIb3"
 + "DQEHAaCCAgEEggH9MIIB+TCCAfUGCyqGSIb3DQEMCgECoIIBEDCCAQwwVwYJ"
 + "KoZIhvcNAQUNMEowKQYJKoZIhvcNAQUMMBwECNhVXbrzrAMNAgIH0DAMBggq"
 + "hkiG9w0CCQUAMB0GCWCGSAFlAwQBKgQQ0BXsfgqcfDzPx5NaTbwA5gSBsLxT"
 + "h8b5ZF/F7oq+45GTsDisspv1ZcJrp5fwg/PWmxA4ao/LarQX+b0xXq+TpCMN"
 + "KtvWG+tRHoJfNcMlKLlppi0LV0WHQ6juj5zvb3lQX8BUvWOjCNz9z20tM5w+"
 + "4lD6vZsDgXEi56feMwprdfw/Vq1mZJ3wBBsoSJtMRZoh580C5ij/BCbczA/O"
 + "mFYGN/Ix9KYzxC/ecPWADbe7C7yVbIS1aNh5EVcSkVNf8zhEdOKNMYHRMBMG"
 + "CSqGSIb3DQEJFTEGBAQBAAAAMFsGCSqGSIb3DQEJFDFOHkwAewA2AEQAQQA2"
 + "AEQARQBGADAALQBFADAARQBDAC0ANAAxAEYANwAtAEEANQA2AEMALQBFADMA"
 + "NQA5AEIAMgBFADEARgBBADQANQB9MF0GCSsGAQQBgjcRATFQHk4ATQBpAGMA"
 + "cgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABv"
 + "AHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIwggjCBgkqhkiG9w0BBwagggiz"
 + "MIIIrwIBADCCCKgGCSqGSIb3DQEHATBXBgkqhkiG9w0BBQ0wSjApBgkqhkiG"
 + "9w0BBQwwHAQIqytIbVkt24ICAgfQMAwGCCqGSIb3DQIJBQAwHQYJYIZIAWUD"
 + "BAEqBBCU1/xxOdG62x7n1Kz/4wPJgIIIQK4qtOJGR/+2+HD1MYVhFGaqYKpB"
 + "wRQ4lg9HI4BYolGqJT19NgpSc95nFfq5nLoCI4nnH7b9U7p2t7wRR08tz5NO"
 + "kPLt2DgOd3cOJCmSv9I40OVBuST6tukzwWi09x7uaOLqATvqfLrISDxYWWID"
 + "O9QXqTLazwtgszk6eorZh9bogoINjdXOsZgDYHfHJvttBITI8fxLbUPjE/Fw"
 + "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"
 + "59no+6YCAgfQ");

        private static readonly byte[] sentrix2 = Base64.Decode(
   "MIIKxwIBAzCCCnMGCSqGSIb3DQEHAaCCCmQEggpgMIIKXDCCAhAGCSqGSIb3"
 + "DQEHAaCCAgEEggH9MIIB+TCCAfUGCyqGSIb3DQEMCgECoIIBEDCCAQwwVwYJ"
 + "KoZIhvcNAQUNMEowKQYJKoZIhvcNAQUMMBwECNLmcXtb3SOSAgIH0DAMBggq"
 + "hkiG9w0CCQUAMB0GCWCGSAFlAwQBKgQQG3jOE88tMcGPYiMn4frFZASBsIXA"
 + "JRyUSyy0UfmoW5093F213ROErwIKSwInMD2x/jAHBOK3tTLGiH0YZyy0kqnO"
 + "BfjbvweJ1FZOLAAfFiERX+hEFJN0yll+mweUU/Yrgyrrmtre4Xn4bJWVN7k1"
 + "j6dU/Ub5p8FMpROmG4biiY1L5GcjOFZCiQiVaSh6FWwsN/qcdaTfJi/mKjsf"
 + "TGk/G/intuvPxTgQC6c/ZY4MXpQZ6CFHf41mF9THgR0Ur+WgNSW7MYHRMBMG"
 + "CSqGSIb3DQEJFTEGBAQBAAAAMFsGCSqGSIb3DQEJFDFOHkwAewA2AEQAQQA2"
 + "AEQARQBGADAALQBFADAARQBDAC0ANAAxAEYANwAtAEEANQA2AEMALQBFADMA"
 + "NQA5AEIAMgBFADEARgBBADQANQB9MF0GCSsGAQQBgjcRATFQHk4ATQBpAGMA"
 + "cgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABv"
 + "AHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIwgghEBgkqhkiG9w0BBwGgggg1"
 + "BIIIMTCCCC0wggLLBgsqhkiG9w0BDAoBA6CCAqMwggKfBgoqhkiG9w0BCRYB"
 + "oIICjwSCAoswggKHMIICLaADAgECAhAqxxC4L9yAIPEUfBRIgsUUMAoGCCqG"
 + "SM49BAMCMIGRMQswCQYDVQQGEwJVUzEUMBIGA1UECwwLRW5naW5lZXJpbmcx"
 + "HTAbBgNVBAoMFERhdGEgSS9PIENvcnBvcmF0aW9uMRAwDgYDVQQHDAdSZWRt"
 + "b25kMRMwEQYDVQQIDApXYXNoaW5ndG9uMSYwJAYDVQQDDB1EYXRhIEkvTyBF"
 + "QyBUZXN0IEludGVybWVkaWF0ZTAeFw0yMDAyMjgyMzA3MzJaFw0yNTAyMjgy"
 + "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"
 + "LOhDDVV1zCxAAQ7XjkwCAgfQ");

        private static readonly byte[] sentrix3 = Base64.Decode(
            "MIILRQIBAzCCCvEGCSqGSIb3DQEHAaCCCuIEggreMIIK2jCCAhAGCSqGSIb3"
        + "DQEHAaCCAgEEggH9MIIB+TCCAfUGCyqGSIb3DQEMCgECoIIBEDCCAQwwVwYJ"
        + "KoZIhvcNAQUNMEowKQYJKoZIhvcNAQUMMBwECKDaE6qqgBaCAgIH0DAMBggq"
        + "hkiG9w0CCQUAMB0GCWCGSAFlAwQBKgQQ8/tQCUjOKfjw9utJiNBAXASBsHmJ"
        + "/o9iTQbJFD0GsBDTyuAoqpC5l6FRr/yuAqRR9sr11qBg3qTsM7bggOjKR35D"
        + "gHq1ODrrMAJQ/esG4xep6kpX7O5W1tGwTgYEQlucUI09Sel7otMJ+JNhKb+h"
        + "6+9bklaNc33FOCnpuyfn/JT0xp1K4NTDIHT1XgTwRyJkaqYssINg3Hap7cyv"
        + "EikgdFc/crpyXMkNwsmvavtYxaqTrLnizSiYgodrx/uMAUY/X1VEMYHRMBMG"
        + "CSqGSIb3DQEJFTEGBAQBAAAAMFsGCSqGSIb3DQEJFDFOHkwAewA2AEQAQQA2"
        + "AEQARQBGADAALQBFADAARQBDAC0ANAAxAEYANwAtAEEANQA2AEMALQBFADMA"
        + "NQA5AEIAMgBFADEARgBBADQANQB9MF0GCSsGAQQBgjcRATFQHk4ATQBpAGMA"
        + "cgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABv"
        + "AHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIwggjCBgkqhkiG9w0BBwagggiz"
        + "MIIIrwIBADCCCKgGCSqGSIb3DQEHATBXBgkqhkiG9w0BBQ0wSjApBgkqhkiG"
        + "9w0BBQwwHAQI5w8DkzTclKsCAgfQMAwGCCqGSIb3DQIJBQAwHQYJYIZIAWUD"
        + "BAEqBBCbqbEsYTwhm5BVxA6YOSbygIIIQAJb+MhOyNMcUoLQiuX1xghRaVlV"
        + "SZ1kHFIe47EUaJehv7iEwbApE9I6W3DkEmshf6UB46Mo7PfhOII9XUCmUbqi"
        + "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"
        + "Jv/0GqsJzTk0Z44YGsD3oZC9vHjLqtGZFJGh4sBBFG79jz1y5yPCICczi1Ob"
        + "m13RnzATdQIlHmGCgn/EYhs7L3aTuZzuouw9UF4H9/9KerDGHuTAXH91rBmM"
        + "A7wbfEPa0ahVRznE6SY40mXnR4v/1OHLqR2LbQXF0yyMD3tUQU4Z5IamLETZ"
        + "4czTGYe4jJ56DHoGCgUM0D7UcPYeCOkJ2dF08KGkaJK7+3m/hC5h5xeji4cZ"
        + "9IWRWl5cs8Knbjj6UzzJoFhLEqAGpk2eVe/37GRBgAEYhYgVxgR6TSVjKTFr"
        + "gR9SVXriBNbBtPXJ0FrnH7mB5CEvZutbWkMdUNhJScd91Hqxepigg8FLHGdk"
        + "WlkyeTQKslfAbsmi9Fts662aH62av5cMAb/qNcXFDHYOaf8C9n0tj6oy3RTl"
        + "3vWBaCHNElYbR5Mr7BgK4NDqf5gu2+7bznxNLQU9OEzF3fCFzk/yXBr1/jnw"
        + "qp6kFDJ6juV6LOJx1GrZAMk4YYk7MbaNWrMhjMtjtgF+yULppQrm1kIS+AvL"
        + "LrRKlFF/VkWHeYNsmZqLfuaYTK7sUG7lEiQJtyBzIMFJR7+4GYbgAjEbHUtU"
        + "bCHLQzAOj3IyJRMBQ4nZwDpjeLQcSg6Hn36ChEEODvjgK6X6itdcpV3HUXEO"
        + "I3QQrVjIwwlzb97t3amLIACawBnS6jBLMC8wCwYJYIZIAWUDBAIBBCAUXt8Q"
        + "ibBrvMZx3gRniSJ3WyTswVw8Uhlh2334lHDeXwQUKbP3m3YDD6TKUgoo7yiq"
        + "V71jzcoCAgfQ");

        private static readonly byte[] repeatedLocalKeyIdPfx = Base64.Decode(
             "MIISUQIBAzCCEhcGCSqGSIb3DQEHAaCCEggEghIEMIISADCCCDcGCSqGSIb3"
            + "DQEHBqCCCCgwgggkAgEAMIIIHQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYw"
            + "DgQICxSuOGaRUU4CAggAgIIH8EeSXfPe3pE5Dzzhpdd1OyFTfQnAMDBkTpxh"
            + "TYnvxZc9uKGmMils1HHS6iKV+VfIyUUvnFoNULy9DIQkkOZK8panWq8ORX+8"
            + "VgdJrlxbUGOB/O4MXgAptWpMHsL4Dc4CvIH3iR5oaRLI53e/9X8Y48T/k8Xx"
            + "lKTS08lM6AP6rHjCEgKtkj4xVU1m+GZibc52edUtW5Tc3731n4n9kf85XGrx"
            + "UJ4+PmwHx0DGjpmBRlAk0/AnQrW4MYIx5tuhR3412M2WaJM2M8pizCcMWVyn"
            + "YlBJ8PkgfS3t5Nq2KJNWAnZj54zfINDKaLNL0SnZakN0+DHiMOB+Y7kFIqKD"
            + "CM6aUP5LBpWYsZKUkTX6gcOOhz0+nzrtmP4tGL3xGpazrrOoZTrzQDZBVv1y"
            + "KOwPoOrv0u44aVoqIXzlAm7VEHSsVkLLQouqsmqWagWypAn2zahVMR5rGflG"
            + "04IgLGBWIU4hPx59TnxyOUtYn2mGS0pK7Xyim7nHVtCC63OFqy/Pm+4TQkLc"
            + "+ZMLs8EdMtXuJSfOE5mMnPPkgwFAjF3MsG8ytjBS1xAdPCqsD4fH6MKeT/fF"
            + "A6SFYd8M0btGa30dBRAVF2DNkF0zUwtTYysh6Z25dgxAf6HKYy+SpqMOJJl9"
            + "eAGULAjFkB5i/PVDPjk7QuipgfVc35/zde38WAKXkfQuhvn9xkjtplRnhkMV"
            + "7Sgvo5Yaaz1Ee8i3VeeR79JXd9UjOSEt62AOb6Isf8hx9v87hcGD4bUQUQNl"
            + "zEHcifkEjRecB6EZHKq8MCAkz53bN2Vr6y3ET2ImBdzaYrKLixsTJNELCzbl"
            + "L+U8JXJr2RcEUOSPW8mmypvpVV5GPmSi72Qx20raTyLhrVLY4yGt+pdNGqSd"
            + "LjegYgSKnYgePXLU8pZVPSCLuZ9Bonv1l2PFPr7dQvrDALk9Q9FYLnAFc7Jf"
            + "zYIiaSNAeiFCGvMpjiNIUMS1bqeAv6bnJg7YqFS24M4gztSxxooTY1L3ILLX"
            + "aJfkYxPbQAonKgKuOHxUM6RHyxPTLLQCmLa2JeyKDEpC0pC+VL6d6UJyOI0e"
            + "o2Lu+Anby5o/fvRtww4pz2yIbMsaEzxMyu+HtWkANo+g3NSxu6VylcrUq/QQ"
            + "r5csLC+37eJyi2OKDKMSUPt3vkVk+5yu830oNHCEhn4kHCISC7rDb0QMfb8F"
            + "hvmZXILcbmdxsKc+4Lzk4rhrsn2xBu4OL2JKNBYlfIV4n5GfPBBBuMzAC6eX"
            + "2OLpaxq/DlbkKzaJtKyj6+npe8CGO/82Yl1OaG6vbLXoSD6DGMR6AHq287tq"
            + "aCSiu5J/a33BUo1DfuEFOscXHipLPvkM5JFx7L/0OfvJcioe1VOtrL9sRZ1F"
            + "8eWSonj2EEidxy6mDhbFDPMqnPo0ETay+VXNVznzmrW7mtuQ0ZOAZ84DMDUy"
            + "JallUgpK1zvwae0sIXdblTN98DGU2hFpXMu9n030BmhkEEJRbKikxpEfWcxI"
            + "0j7+276v6Z32C4BpoeGFxpfrpozwJzTBoGlkN408thpB/llqkBSeLtxEnSKO"
            + "SbpKquR6kg4FYQwq7pbKQuSjvKztIhdCDanmi+cPNppHyGmbf4yDMtWOLGhU"
            + "cLUbdMJ8jWcvDZiKwzB4s1Qka1Z8qVi5VVEPhpzh4E89WqSoRtgLbhkoy8mf"
            + "BlptXBWggXs+eR/s2YXb3nF2ZPnYQcn1+alCMM8aXf9HEeaFGEsEAsANglu9"
            + "lnlaGFLbzIeGwkpGFLVlj9Fd+s+a6OVmeaPCa1cZaTKzdC6FqHd3bxalLBc8"
            + "fDcbJPDYsTYFEajw648HG3x3oyk+P5WOa9ULup6emu/zOhH5VPJcj9CniLqq"
            + "EZDaIDPRqu0g5MZir0EdXL6FvRU2o7W8JE7fzSYxlAWcUbuY6TdaGO7SOvxq"
            + "oRKpsIzcmBLpw1t5djii/SohQR/2mJhvqETWvOerfEQr3vXnwurVUC1u/vEO"
            + "lCqCRNLH0qzQUFrwzm/kMWsaDMb85XYQpZ+StT6n5AHWNWuQG3bmZIjAY9R9"
            + "AbPWi/eSYQF/1E4qZ6WE+S2VOg3i/iVT0MtJzuGXRl1rycZv2vPiskEvVGhT"
            + "DbqPWapGugbXLIne6b0C83EtK9H8s/TdDcRupjzW9J/8+p8+DJROgg6y2wIm"
            + "SiUKHMZsuU9+e5+z87HVB9t2y0RsARJdbB9NhuTZk7ELztNponTMdJmUyIT0"
            + "phdz3U9MFEWnHccMMGo3KqopOsrGT9YqdcBSSfSOyckVBQzgzVt8ypWqusW5"
            + "j53OudlwcoudmOTdUfYoEuDWGYgoGqSEOnaU8dqkl4ZffYUIeJuTwclJd9N+"
            + "onB82b5FpzXo+sO6DWpNJ8lGE2mOqt4H/HvRwTJ5pfXlZSmGom4eq+WU6XfB"
            + "CRE6ehopnU91wey0/lceYPshOQriqoVE577xJKU3zWuWzFwBvanrN2YxptdV"
            + "xNiKabM3UmSxgGtxITEb1pCAGBTZzBL175CvmBF6VyamVe4YH7wuQ+C2jEle"
            + "xH6xo+1eWyPT5L5CSVFgiJpxX2WoI/8qvnSdagJW0+IQWk1nfNJb2aNbBu4z"
            + "Iw6txeGxtQHPhF5eyVgRDE6OannycbSOJMi14q8n4zhyJebsY1wldB31XOFV"
            + "pnCRNUcMcueAMioxliO7K81O8SiQVbIyVsc2GYQqhevdDHwj2azat60kNytq"
            + "nimsplgyD84oeMMwaqbciAOItPbdqbz7zGIwggnBBgkqhkiG9w0BBwGgggmy"
            + "BIIJrjCCCaowggmmBgsqhkiG9w0BDAoBAqCCCW4wgglqMBwGCiqGSIb3DQEM"
            + "AQMwDgQIGF7xpMpvXmgCAggABIIJSD9TY2dCx+BYL555TLN51ksuTf1NkXjD"
            + "Az1uymDqjoQ1R67CbcHU2qPKwIW3fZT7OmpiPI8cgogKBvcFnngmfeuvZYST"
            + "gOW2iD83m5PlaoIa2iXjqdPdCQF9rVwkBg6VUYWlVd9qR0LPKT3u3UsScN2d"
            + "qPeeSVRL8W4ESR6OMgiEIts50cjUwb2acqy3So9LZU77/D6uynj5+iFwYxlb"
            + "yX/70oegWqMZyiZfvGT1UNMQxF/hs0E7ZGW1RnWjixVI+p3Pvqw36mH9j3Xn"
            + "ms5Eg4mhbyBXRPIsq3/95O5NRh84R9atvR+zn1Pd59DKIUVf11w6HZoNOtH+"
            + "XrVG7MpLVFIbgxZWzr6JIOK7/bMkJi+aBwSfm3yR6U46XDYlDHfSeeL/xJFm"
            + "0QSNf6S/K7lj+AwAoOrruaxXwHxO1zlaXGo8R8P5JVgr7/mbsMF2RnXp3JrW"
            + "KIC5LsePgyy6bYBdFwbJZ/FQVFp0zCnByUvPP8xS9+TdZKvp9u6rgIzNCPFK"
            + "m1f9XkgJhIn4TEgRVp/OGYbUX2g4Gbm15TUUwyXUFsas224Cyv2RwrzpKNXv"
            + "SpriMdXtbEIzt6IRKH6oB1x5QbPz7cJhKL8pbhOUnjSourJC5S8LgazqyKaS"
            + "VIon108lGcq6xJJKySxsA74luILEpQyTgZfpSUblCeRcOMdXW0OA+K6PhXqa"
            + "e1EzWHo6F7iMTi0SSZ/XOV6Px9xV3e2vRXSt13+pzTpRZKpzVHjpkiWz7xYt"
            + "NI2Vq+4LB/ZCi3jhJLYoRYSP9XEf2ilfplK4rGD8tIEDTkFLnKv8oKA+dnwX"
            + "VNxKoJQnPuYLTDCU/KfODU4gAyefuTCH/8iAry/6UhOM9Pr50boQJ9KK5Ckz"
            + "lkptnLqP6vCSFexkETdh+Vx2Jf87EmytpUhJXS1i2EOjihWH0hEUknEip7km"
            + "a1m489qAMUzOMZ2RaIu0p/gDGlLkLem2yv6AOFcqBpQWo0yc4HaPf1Op06/7"
            + "ZkykXPyekYI0kKKMrILrW7XifSyvAaPiuXIRzH7GpRIoPmNEEnm3jSpPfkZo"
            + "ttCWxzktVVFD2ACTQXV6uDH6eiOrZpMtrg6tOqfbgdGkohfjLtnSjjfhAgTg"
            + "N+Tb7w9FgGMZjvrA9Ht50m1iqGg25la5jBfCaD1ZOoAWe1mhRiWVFuEuVewb"
            + "dA1hny6OSXxc+5v6pyXKYodMDjYW8m3hhgQfxo1vAoOIy9UWiZ+kVQbw68MO"
            + "G7mOYw4WKKDPw4LaiPVSexmu08mYJXiOU+mNlvv0GseQ6gK+sDsQKiPa8KGi"
            + "JllcaKnJ9qmO0ff5IZ2bDyAESkUgwb9CaT7yjTz+z78Q/IcAVafYLkbJb4FZ"
            + "UFUTsLfw172NW1MzTeyfHe9YtZsOUuViuFLrKoWO8fDRYcA65J30C1S8P3bU"
            + "aauukUZBoFndn6sz0DH6z+YLOjvoAlyze+692saz6OPf8iYPacq25i44He2v"
            + "sRpbqrWw3Xn1fcd2WQABQ4Ig4sntuuvG6ZYFl9//8ZtNfe6hS9iZzyP4CPiz"
            + "Q9cD15Xln24gW2EZuJ6yfgiK8AYfZIUmGu0fUO4x5kPNbkIEdb+a2HYDb4rs"
            + "m8dFVKCKDPOjEojkfFz3DXycQn79xOxs7HEg7GZtV6/jnu0tel2qHQNBPM2i"
            + "1RWHfiC+/kKWmiQlwbSYzyFcbwcPYMzGVcbJoL/SidqxcD4PeD7PnPmAHzy9"
            + "2rvGPu6m0/7HhjsMicG/fyfczY+SPSkxoxa2N46yS397tKbUVJ9SfUATJ0vM"
            + "YqnqqgKQTd0MzdQNcNOE6EozK3kCTV+6xjs1aSvwB51gp03vcw+Ln0mW4Dfd"
            + "mM+HwFre9w37DJBajEXbzkI6aHApCQdN6VexsaSyxFewwavTqwmVeSTIflpa"
            + "s2lFbJmLbMDcdgiIHyvOygg0oXy1JuFScuBZ4T21kcTAEX/hFPfLTGTUXeBA"
            + "hMY+gkdol/IB9gNPL23KLskXv4+Z9sCVheG3eAi4qeLgkqr86L8aIYa0Akdl"
            + "UnxREE0/sDeiqaV+fg/cHGa51YRVHhyG4/+//PQ46xW5ORhGnOSs7UHZB1XK"
            + "5ROgOFDe1HpKhQwWbFEaMY+CLSBFDNmMZiaoArf1r5qIqwG6fy++T0f7plDm"
            + "CdoZcV5IyIwmVwzZu6mMz7MRPEBYH/WitC/lP4jdtHqtHqKtPT+NXHzh2A/c"
            + "tl3nOSR5RnDKzL7kUVXheL2cES+41jZ1LBy9T3ZxXd59Tf8IdC1JJ24pHmzJ"
            + "KZNYxIr+rREvIxZsNvJI2urefS2/55AZCVC58hahjS4DWPzpxMkRb5xkL8L8"
            + "Yt8OikvdsazHuamTlh2tggCiQ3LDqXtG+sk67UZQycWis0vG6pd6S+E6K4Sn"
            + "mymsn/CbsSqr8rVd/JppqmYnPPmzkEw0oztNW2pNzfui+vvr6gFV81cWIdFt"
            + "TOWlXSzOAwm7/Dj8kSYyIxZVrHsRCbnSdkmP0timKpBkGN+CCHsibzl0RJHj"
            + "IIznWLUBCnwxUwLJCdo6qTZoOhegDYZFVgk4eExiL26XJjbLJj25tB+ITBiI"
            + "RwR52Mxf9zoX3V8I1OCnvznt0fE5NBTob0LDXoqGy1PlTJv+8DKrI4Vzgk9/"
            + "yDjMw/NJgimQS97cxU9CxnQO7FmCEnEeB/E9kniml5EGLEod3Ugf6ElcuADn"
            + "Kp4E1cijOiRdHPX6vnxjw0n0OHpOpaCf9ynWF5fVXNoAofqTB7nGMn1YPJUo"
            + "gl2pDlX6Z5dTZ1IyoOAxkOLTKH6ULY91wnPgC1k8LoOKAWJOtcdxBai+rxGe"
            + "Nw6pYsl6L0ViIBsVHOlg+JEFtVEhL9vUuv9NcqWu6swt/bV84rU993vPj1FR"
            + "bPasRjTBAIwJ4KVvQHtXg0bta8vWFIt7hOSjpKD+Fe8c59oZcmvmfCWo1yJE"
            + "YMGZETkP58SlGtVUMOXYZxEzLe1OG2icd8guZ4d/DeCLv6JuO2q958UdM1I6"
            + "yO8+1r1w6nzsTwaXZn0htKYbC8dYIl7jJK/5y/ITZWSJl5BbtoUXlVuz/GGx"
            + "vllOfG+H/iSr3wQ8Meue+9zh1l+gdH10dWlLCjot0ri8e/xKU9ZaRt6dlHka"
            + "c8Nh1U30ZrMBooXYBZmA/0+Ntz5WIEh6GQKeWV/ZwtOZdfGsYzC1L9t3Xknu"
            + "GxVjEfV1l7b9Fx5j5jElMCMGCSqGSIb3DQEJFTEWBBRFy/ERb7PziymEs8ci"
            + "TK5wp093iTAxMCEwCQYFKw4DAhoFAAQU1SGg9xV7jfLcJh3tzd+phZTMN38E"
            + "CL6WgCtEom7kAgIIAA==");

        private readonly SecureRandom Random = new SecureRandom();

        /**
		* we generate a self signed certificate for the sake of testing - RSA
		*/
        public X509CertificateEntry CreateCert(
            AsymmetricKeyParameter pubKey,
            AsymmetricKeyParameter privKey,
            string issuerEmail,
            string subjectEmail)
        {
            //
            // distinguished name table.
            //
            IDictionary issuerAttrs = new Hashtable();
            issuerAttrs.Add(X509Name.C, "AU");
            issuerAttrs.Add(X509Name.O, "The Legion of the Bouncy Castle");
            issuerAttrs.Add(X509Name.L, "Melbourne");
            issuerAttrs.Add(X509Name.ST, "Victoria");
            issuerAttrs.Add(X509Name.EmailAddress, issuerEmail);

            IDictionary subjectAttrs = new Hashtable();
            subjectAttrs.Add(X509Name.C, "AU");
            subjectAttrs.Add(X509Name.O, "The Legion of the Bouncy Castle");
            subjectAttrs.Add(X509Name.L, "Melbourne");
            subjectAttrs.Add(X509Name.ST, "Victoria");
            subjectAttrs.Add(X509Name.EmailAddress, subjectEmail);

            IList order = new ArrayList();
            order.Add(X509Name.C);
            order.Add(X509Name.O);
            order.Add(X509Name.L);
            order.Add(X509Name.ST);
            order.Add(X509Name.EmailAddress);

            //
            // extensions
            //

            //
            // create the certificate - version 3
            //
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();

            certGen.SetSerialNumber(BigInteger.One);
            certGen.SetIssuerDN(new X509Name(order, issuerAttrs));
            certGen.SetNotBefore(DateTime.UtcNow.AddDays(-30));
            certGen.SetNotAfter(DateTime.UtcNow.AddDays(30));
            certGen.SetSubjectDN(new X509Name(order, subjectAttrs));
            certGen.SetPublicKey(pubKey);
            certGen.SetSignatureAlgorithm("MD5WithRSAEncryption");

            return new X509CertificateEntry(certGen.Generate(privKey));
        }

        private void DoTestCertsOnly()
        {
            Pkcs12Store pkcs12 = new Pkcs12StoreBuilder().Build();

            pkcs12.Load(new MemoryStream(certsOnly, false), null);

            IsTrue(pkcs12.ContainsAlias("alias"));

            MemoryStream bOut = new MemoryStream();

            pkcs12.Save(bOut, null, Random);

            pkcs12 = new Pkcs12StoreBuilder().Build();

            pkcs12.Load(new MemoryStream(bOut.ToArray(), false), null);

            IsTrue(pkcs12.ContainsAlias("alias"));

            try
            {
                pkcs12.Load(new MemoryStream(certsOnly, false), "1".ToCharArray());
                Fail("no exception");
            }
            catch (IOException e)
            {
                IsEquals("password supplied for keystore that does not require one", e.Message);
            }

            // TODO Modify environment variables in tests?
            //System.setProperty(Pkcs12Store.IgnoreUselessPasswordProperty, "true");

            //pkcs12.Load(new MemoryStream(certsOnly, false), "1".ToCharArray());

            //System.setProperty(Pkcs12Store.IgnoreUselessPasswordProperty, "false");
        }

        private void DoTestPkcs12Store()
        {
            BigInteger mod = new BigInteger("bb1be8074e4787a8d77967f1575ef72dd7582f9b3347724413c021beafad8f32dba5168e280cbf284df722283dad2fd4abc750e3d6487c2942064e2d8d80641aa5866d1f6f1f83eec26b9b46fecb3b1c9856a303148a5cc899c642fb16f3d9d72f52526c751dc81622c420c82e2cfda70fe8d13f16cc7d6a613a5b2a2b5894d1", 16);

            MemoryStream stream = new MemoryStream(pkcs12, false);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, passwd);

            string pName = null;
            foreach (string n in store.Aliases)
            {
                if (store.IsKeyEntry(n))
                {
                    pName = n;
                    //break;
                }
            }

            AsymmetricKeyEntry key = store.GetKey(pName);

            if (!((RsaKeyParameters)key.Key).Modulus.Equals(mod))
            {
                Fail("Modulus doesn't match.");
            }

            X509CertificateEntry[] ch = store.GetCertificateChain(pName);

            if (ch.Length != 3)
            {
                Fail("chain was wrong length");
            }

            if (!ch[0].Certificate.SerialNumber.Equals(new BigInteger("96153094170511488342715101755496684211")))
            {
                Fail("chain[0] wrong certificate.");
            }

            if (!ch[1].Certificate.SerialNumber.Equals(new BigInteger("279751514312356623147411505294772931957")))
            {
                Fail("chain[1] wrong certificate.");
            }

            if (!ch[2].Certificate.SerialNumber.Equals(new BigInteger("11341398017")))
            {
                Fail("chain[2] wrong certificate.");
            }

            //
            // save test
            //
            MemoryStream bOut = new MemoryStream();
            store.Save(bOut, passwd, Random);

            stream = new MemoryStream(bOut.ToArray(), false);
            store.Load(stream, passwd);

            key = store.GetKey(pName);

            if (!((RsaKeyParameters)key.Key).Modulus.Equals(mod))
            {
                Fail("Modulus doesn't match.");
            }

            store.DeleteEntry(pName);

            if (store.GetKey(pName) != null)
            {
                Fail("Failed deletion test.");
            }

            //
            // cert chain test
            //
            store.SetCertificateEntry("testCert", ch[2]);

            if (store.GetCertificateChain("testCert") != null)
            {
                Fail("Failed null chain test.");
            }

            //
            // UTF 8 single cert test
            //
            stream = new MemoryStream(certUTF, false);
            store.Load(stream, "user".ToCharArray());

            if (store.GetCertificate("37") == null)
            {
                Fail("Failed to find UTF cert.");
            }

            //
            // try for a self generated certificate
            //
            RsaKeyParameters pubKey = new RsaKeyParameters(
                false,
                new BigInteger("b4a7e46170574f16a97082b22be58b6a2a629798419be12872a4bdba626cfae9900f76abfb12139dce5de56564fab2b6543165a040c606887420e33d91ed7ed7", 16),
                new BigInteger("11", 16));

            RsaPrivateCrtKeyParameters privKey = new RsaPrivateCrtKeyParameters(
                new BigInteger("b4a7e46170574f16a97082b22be58b6a2a629798419be12872a4bdba626cfae9900f76abfb12139dce5de56564fab2b6543165a040c606887420e33d91ed7ed7", 16),
                new BigInteger("11", 16),
                new BigInteger("9f66f6b05410cd503b2709e88115d55daced94d1a34d4e32bf824d0dde6028ae79c5f07b580f5dce240d7111f7ddb130a7945cd7d957d1920994da389f490c89", 16),
                new BigInteger("c0a0758cdf14256f78d4708c86becdead1b50ad4ad6c5c703e2168fbf37884cb", 16),
                new BigInteger("f01734d7960ea60070f1b06f2bb81bfac48ff192ae18451d5e56c734a5aab8a5", 16),
                new BigInteger("b54bb9edff22051d9ee60f9351a48591b6500a319429c069a3e335a1d6171391", 16),
                new BigInteger("d3d83daf2a0cecd3367ae6f8ae1aeb82e9ac2f816c6fc483533d8297dd7884cd", 16),
                new BigInteger("b8f52fc6f38593dabb661d3f50f8897f8106eee68b1bce78a95b132b4e5b5d19", 16));

            X509CertificateEntry[] chain = new X509CertificateEntry[] {
                CreateCert(pubKey, privKey, "issuer@bouncycastle.org", "subject@bouncycastle.org")
            };

            store = new Pkcs12StoreBuilder().Build();

            store.SetKeyEntry("privateKey", new AsymmetricKeyEntry(privKey), chain);

            if (!store.ContainsAlias("privateKey") || !store.ContainsAlias("PRIVATEKEY"))
            {
                Fail("couldn't find alias privateKey");
            }

            if (store.IsCertificateEntry("privateKey"))
            {
                Fail("key identified as certificate entry");
            }

            if (!store.IsKeyEntry("privateKey") || !store.IsKeyEntry("PRIVATEKEY"))
            {
                Fail("key not identified as key entry");
            }

            if (!"privateKey".Equals(store.GetCertificateAlias(chain[0].Certificate)))
            {
                Fail("Did not return alias for key certificate privateKey");
            }

            MemoryStream store1Stream = new MemoryStream();
            store.Save(store1Stream, passwd, Random);
            DoTestNoExtraLocalKeyID(store1Stream.ToArray());

            //
            // no friendly name test
            //
            stream = new MemoryStream(pkcs12noFriendly, false);
            store.Load(stream, noFriendlyPassword);

            pName = null;

            foreach (string n in store.Aliases)
            {
                if (store.IsKeyEntry(n))
                {
                    pName = n;
                    //break;
                }
            }

            ch = store.GetCertificateChain(pName);

            //for (int i = 0; i != ch.Length; i++)
            //{
            //	Console.WriteLine(ch[i]);
            //}

            if (ch.Length != 1)
            {
                Fail("no cert found in pkcs12noFriendly");
            }

            //
            // failure tests
            //
            ch = store.GetCertificateChain("dummy");

            store.GetCertificateChain("DUMMY");

            store.GetCertificate("dummy");

            store.GetCertificate("DUMMY");

            //
            // storage test
            //
            stream = new MemoryStream(pkcs12StorageIssue, false);
            store.Load(stream, storagePassword);

            pName = null;

            foreach (string n in store.Aliases)
            {
                if (store.IsKeyEntry(n))
                {
                    pName = n;
                    //break;
                }
            }

            ch = store.GetCertificateChain(pName);
            if (ch.Length != 2)
            {
                Fail("Certificate chain wrong length");
            }

            store.Save(new MemoryStream(), storagePassword, Random);

            //
            // basic certificate check
            //
            store.SetCertificateEntry("cert", ch[1]);

            if (!store.ContainsAlias("cert") || !store.ContainsAlias("CERT"))
            {
                Fail("couldn't find alias cert");
            }

            if (!store.IsCertificateEntry("cert") || !store.IsCertificateEntry("CERT"))
            {
                Fail("cert not identified as certificate entry");
            }

            if (store.IsKeyEntry("cert") || store.IsKeyEntry("CERT"))
            {
                Fail("cert identified as key entry");
            }

            if (!store.IsEntryOfType("cert", typeof(X509CertificateEntry)))
            {
                Fail("cert not identified as X509CertificateEntry");
            }

            if (!store.IsEntryOfType("CERT", typeof(X509CertificateEntry)))
            {
                Fail("CERT not identified as X509CertificateEntry");
            }

            if (store.IsEntryOfType("cert", typeof(AsymmetricKeyEntry)))
            {
                Fail("cert identified as key entry via AsymmetricKeyEntry");
            }

            if (!"cert".Equals(store.GetCertificateAlias(ch[1].Certificate)))
            {
                Fail("Did not return alias for certificate entry");
            }

            //
            // test restoring of a certificate with private key originally as a ca certificate
            //
            store = new Pkcs12StoreBuilder().Build();

            store.SetCertificateEntry("cert", ch[0]);

            if (!store.ContainsAlias("cert") || !store.ContainsAlias("CERT"))
            {
                Fail("restore: couldn't find alias cert");
            }

            if (!store.IsCertificateEntry("cert") || !store.IsCertificateEntry("CERT"))
            {
                Fail("restore: cert not identified as certificate entry");
            }

            if (store.IsKeyEntry("cert") || store.IsKeyEntry("CERT"))
            {
                Fail("restore: cert identified as key entry");
            }

            if (store.IsEntryOfType("cert", typeof(AsymmetricKeyEntry)))
            {
                Fail("restore: cert identified as key entry via AsymmetricKeyEntry");
            }

            if (store.IsEntryOfType("CERT", typeof(AsymmetricKeyEntry)))
            {
                Fail("restore: cert identified as key entry via AsymmetricKeyEntry");
            }

            if (!store.IsEntryOfType("cert", typeof(X509CertificateEntry)))
            {
                Fail("restore: cert not identified as X509CertificateEntry");
            }

            //
            // test of reading incorrect zero-length encoding
            //
            stream = new MemoryStream(pkcs12nopass, false);
            store.Load(stream, "".ToCharArray());

            stream = new MemoryStream(sentrixHard, false);
            store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, "0000".ToCharArray());
            CheckPKCS12(store);

            stream = new MemoryStream(sentrixSoft, false);
            store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, "0000".ToCharArray());
            CheckPKCS12(store);

            stream = new MemoryStream(sentrix1, false);
            store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, "0000".ToCharArray());
            CheckPKCS12(store);

            stream = new MemoryStream(sentrix2, false);
            store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, "0000".ToCharArray());
            CheckPKCS12(store);

            stream = new MemoryStream(sentrix3, false);
            store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, "0000".ToCharArray());
            CheckPKCS12(store);
        }

        private void CheckPKCS12(Pkcs12Store store)
        {
            foreach (string alias in store.Aliases)
            {
                if (store.IsKeyEntry(alias))
                {
                    AsymmetricKeyEntry ent = store.GetKey(alias);
                    X509CertificateEntry[] crts = store.GetCertificateChain(alias);
                }
                else if (store.IsCertificateEntry(alias))
                {
                    X509CertificateEntry crt = store.GetCertificate(alias);
                }
            }
        }
        private void DoTestSupportedTypes(AsymmetricKeyEntry privKey, X509CertificateEntry[] chain)
        {
            basicStoreTest(privKey, chain,
                PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc,
                PkcsObjectIdentifiers.PbewithShaAnd40BitRC2Cbc);
            basicStoreTest(privKey, chain,
                PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc,
                PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc);
        }

        private void basicStoreTest(AsymmetricKeyEntry privKey, X509CertificateEntry[] chain,
            DerObjectIdentifier keyAlgorithm, DerObjectIdentifier certAlgorithm)
        {
            Pkcs12Store store = new Pkcs12StoreBuilder()
                .SetKeyAlgorithm(keyAlgorithm)
                .SetCertAlgorithm(certAlgorithm)
                .Build();

            store.SetKeyEntry("key", privKey, chain);

            MemoryStream bOut = new MemoryStream();

            store.Save(bOut, passwd, Random);

            store.Load(new MemoryStream(bOut.ToArray(), false), passwd);

            AsymmetricKeyEntry k = store.GetKey("key");

            if (!k.Equals(privKey))
            {
                Fail("private key didn't match");
            }

            X509CertificateEntry[] c = store.GetCertificateChain("key");

            if (c.Length != chain.Length || !c[0].Equals(chain[0]))
            {
                Fail("certificates didn't match");
            }

            // check attributes
            Pkcs12Entry b1 = k;
            Pkcs12Entry b2 = chain[0];

            if (b1[PkcsObjectIdentifiers.Pkcs9AtFriendlyName] != null)
            {
                DerBmpString name = (DerBmpString)b1[PkcsObjectIdentifiers.Pkcs9AtFriendlyName];

                if (!name.Equals(new DerBmpString("key")))
                {
                    Fail("friendly name wrong");
                }
            }
            else
            {
                Fail("no friendly name found on key");
            }

            if (b1[PkcsObjectIdentifiers.Pkcs9AtLocalKeyID] != null)
            {
                Asn1OctetString id = (Asn1OctetString)b1[PkcsObjectIdentifiers.Pkcs9AtLocalKeyID];

                if (!id.Equals(b2[PkcsObjectIdentifiers.Pkcs9AtLocalKeyID]))
                {
                    Fail("local key id mismatch");
                }
            }
            else
            {
                Fail("no local key id found");
            }

            //
            // check algorithm types.
            //
            Pfx pfx = Pfx.GetInstance(bOut.ToArray());

            ContentInfo cInfo = pfx.AuthSafe;

            Asn1OctetString auth = (Asn1OctetString)cInfo.Content;

            Asn1Sequence s1 = Asn1Sequence.GetInstance(auth.GetOctets());

            ContentInfo c1 = ContentInfo.GetInstance(s1[0]);
            ContentInfo c2 = ContentInfo.GetInstance(s1[1]);

            SafeBag sb = SafeBag.GetInstance(Asn1Sequence.GetInstance(((Asn1OctetString)c1.Content).GetOctets())[0]);

            EncryptedPrivateKeyInfo encInfo = EncryptedPrivateKeyInfo.GetInstance(sb.BagValue);

            // check the key encryption
            if (!encInfo.EncryptionAlgorithm.Algorithm.Equals(keyAlgorithm))
            {
                Fail("key encryption algorithm wrong");
            }

            // check the certificate encryption
            EncryptedData cb = EncryptedData.GetInstance(c2.Content);

            if (!cb.EncryptionAlgorithm.Algorithm.Equals(certAlgorithm))
            {
                Fail("cert encryption algorithm wrong");
            }
        }

        private void DoTestNoExtraLocalKeyID(byte[] store1data)
        {
            IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            kpg.Init(new RsaKeyGenerationParameters(
                BigInteger.ValueOf(0x10001), Random, 512, 25));

            AsymmetricCipherKeyPair newPair = kpg.GenerateKeyPair();

            Pkcs12Store store1 = new Pkcs12StoreBuilder().Build();
            store1.Load(new MemoryStream(store1data, false), passwd);

            Pkcs12Store store2 = new Pkcs12StoreBuilder().Build();

            AsymmetricKeyEntry k1 = store1.GetKey("privatekey");
            X509CertificateEntry[] chain1 = store1.GetCertificateChain("privatekey");

            X509CertificateEntry[] chain2 = new X509CertificateEntry[chain1.Length + 1];

            Array.Copy(chain1, 0, chain2, 1, chain1.Length);

            chain2[0] = CreateCert(newPair.Public, k1.Key, "subject@bouncycastle.org", "extra@bouncycaste.org");

            if (chain1[0][PkcsObjectIdentifiers.Pkcs9AtLocalKeyID] == null)
            {
                Fail("localKeyID not found initially");
            }

            store2.SetKeyEntry("new", new AsymmetricKeyEntry(newPair.Private), chain2);

            MemoryStream bOut = new MemoryStream();

            store2.Save(bOut, passwd, Random);

            store2.Load(new MemoryStream(bOut.ToArray(), false), passwd);

            chain2 = store2.GetCertificateChain("new");

            if (chain2[1][PkcsObjectIdentifiers.Pkcs9AtLocalKeyID] != null)
            {
                Fail("localKeyID found after save");
            }
        }

        private void DoTestLoadRepeatedLocalKeyID()
        {
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(new MemoryStream(repeatedLocalKeyIdPfx, false), "".ToCharArray());

            IsTrue(store.GetCertificateChain("d4be139f9db456d225a8dcd2969479d960d2514a") == null);
            IsTrue(store.GetCertificateChain("45cbf1116fb3f38b2984b3c7224cae70a74f7789").Length == 1);
        }

        public override string Name
        {
            get { return "PKCS12Store"; }
        }

        public override void PerformTest()
        {
            DoTestCertsOnly();
            DoTestPkcs12Store();
            DoTestLoadRepeatedLocalKeyID();
        }

        public static void Main(
            string[] args)
        {
            RunTest(new Pkcs12StoreTest());
        }

        [Test]
        public void TestFunction()
        {
            string resultText = Perform().ToString();

            Assert.AreEqual(Name + ": Okay", resultText);
        }
    }
}
