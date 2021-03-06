from tempfile import mkstemp
from shutil import move
from os import remove, close

def replace(file_path, pattern, subst):
    #Create temp file
    fh, abs_path = mkstemp()
    with open(abs_path,'w') as new_file:
        with open(file_path) as old_file:
            for line in old_file:
                new_file.write(line.replace(pattern, subst))
    close(fh)
    #Remove original file
    remove(file_path)
    #Move new file
    move(abs_path, file_path)

replace('./com/fasterxml/jackson/core/ObjectCodec.cs', 'public abstract override T readTree<T>(com.fasterxml.jackson.core.JsonParser jp)', 'public abstract override T readTree<T>(com.fasterxml.jackson.core.JsonParser jp);')
replace('./com/fasterxml/jackson/core/ObjectCodec.cs', 'where T : com.fasterxml.jackson.core.TreeNode;', '//where T : com.fasterxml.jackson.core.TreeNode;')

replace('./com/fasterxml/jackson/core/format/InputAccessor.cs','public interface InputAccessor', 'public class InputAccessor')

replace('./com/fasterxml/jackson/core/JsonGenerator.cs', 'public abstract class JsonGenerator : System.IDisposable, java.io.Flushable, com.fasterxml.jackson.core.Versioned', 'public abstract class JsonGenerator : System.IDisposable, com.fasterxml.jackson.core.Versioned')
