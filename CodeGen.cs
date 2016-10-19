using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language {

    public enum InstructionOpcode {
        push,
        pop,
        load,
        store
    }

    public class Instruction {
        public InstructionOpcode Opcode { get; set; }

        public string Parameter { get; set; }

        public Instruction(InstructionOpcode opcode, string parameter=null) {
            Opcode = opcode;
            Parameter = parameter;
        }

    }

    public class CodeGen {

        public List<Instruction> instructions { get; set; }

        private int _label = 0;

        public int Label {
            get {
                return _label++;
            }
        }

        public void integer_immediate(IntegerNode node) {
            Instruction instruction = new Instruction(InstructionOpcode.push, node.Number);
            instructions.Add(instruction);
        }

        public void float_immediate(FloatNode node) {
            Instruction instruction = new Instruction(InstructionOpcode.push, node.Number);
        }

    }
}
