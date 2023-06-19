def hex2dec(s):
        return int(s, 16)
    
def hex2chr(hex):
    out = ''
    for i in range(0,len(hex),2):
        print(hex[i:i+2])
        print(hex2dec(hex[i:i+2]))
        print(chr(hex2dec(hex[i:i+2])))
        out += chr(hex2dec(hex[i:i+2]))
    return out

print(hex2dec("0200018584"))
print('-'+hex2chr("0200018584")+'-')
