using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language {

    public enum DataType {
        VoidType,
        StringType,
        IntegerType,
        FloatType,
        BooleanType,
        CallableType,
        IndexableType,
        NoneType
    }

    public class CallableType {
        public DataType ReturnType { get; set; }
        public List<Node> ParameterList { get; set; }
        public Node FunctionBody { get; set; }

        public CallableType(DataType returnType, List<Node> parameterList, Node functionBody) {
            ReturnType = returnType;
            ParameterList = parameterList;
            FunctionBody = functionBody;
        }

    }


}
