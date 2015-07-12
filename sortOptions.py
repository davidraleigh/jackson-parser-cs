f = open("sharpen-all-options", "r")
# omit empty lines and lines containing only whitespace
lines = [line for line in f if line.strip()]
lines.sort()
f.close()
open('sharpen-all-options', 'w').close()
f = open("sharpen-all-options", "w")
for item in lines:
  f.write("%s" % item)
f.write('\n')
f.close()
