



// Central Control FSM- HeartBeat
module ctr2RecT(clk, reset, // standart inputs
					fail, save, rollBack, // to manipulate Memory blocks
					readBuff, // to manipulte the reading of Input buffer
					substr // to manipulate the output buffer
					);
	input clk; //global synch. clock
	input reset; //global reset signal
	input fail; // error detection wire
	output save;// to manipulate Memory blocks
	output rollBack; 
	output readBuff; // to manipulte the reading of Input buffer
	output substr;  // to manipulate the output buffer
	//Registers
	reg [3:0] state;
	
	reg rollBack; // to manipulate Memory blocks
	reg save;
	reg readBuff; // to manipulte the reading of Input buffer
	reg substr; // to manipulate the output buffer
	//Normal mode
		parameter phase1=		3'h0; //1st phase -error detection
		parameter phase2=		3'h1; //2nd phase - check if errorDetected
	//Recovery procedure
		parameter rollBackF=      3'h2;
		parameter speed1=      3'h3;
		parameter speed2 =    	3'h4;
		parameter speed3 =       3'h5; // last speed- just because d2 
always @ (posedge clk or posedge reset)
	begin 
		if (reset)
			begin
				state=phase1;
				
				rollBack<=0; 
				save<=0;
				readBuff<=0;
				substr <=0;
			end
		else 
			begin: fsm
				case (state)
					phase1: //odd cycle 1,3,5,... when we check fail
						if (fail==1)
							begin
								state=rollBackF;
								
								save<=1;
								rollBack<=1;
								readBuff<=1;
								substr <=1;
							end
						else
							begin
								state=phase2;
								save<=1;
							end
					phase2: // even cycles 2,4,6,... when we don't care
						begin
							state= phase1;
							save<=0;
						end
					rollBackF:
						begin //rollBack=1; save:1<-0; readBuff=1; subst=1
							state=speed1;
							save<=0;
						end
					speed1:
						begin //rollBack=1; save=0; readBuff=1<-0; subst=1
							state=speed2;
							readBuff<=0;
						end
					speed2:
						begin //rollBack=1; save=0; readBuff=0; subst=(1<-0;0)
							state=speed3;
							substr<=0;
						end
					speed3:
						begin
							state=phase2;
							rollBack<=0;
							save<=1;
						end
				endcase
			end
	end
endmodule

//NOTE; such voting will save only agains 1 SEU, synchronization is not established
//TODO: create re-synchronization between counters-> should be efficient
//ctr[3]||reset dropped frequency from 361 to <150Mhz-> so no the best choice

// module ctr2RecTmr(clk, reset, ctr, errFlag); // TMR counter for TTR ctr signals
	// input	clk; //global synch. clock
	// input 	[2:0]reset; //global reset sygnal
	// output	[1:0]ctr; //control outpus for Time Redundancy
	// input	[2:0]errFlag;// Error Flag- indicates that the recovery procedure is in progress and 00 in ctr is required
	
	// wire [5:0]ctrL;
	// // counters instantiation
	// genvar i;
	// generate
		// for (i=0;i<3; i=i+1)
		// begin: ripple
			// ctr2RecT count(clk, reset[i],ctrL[1+2*i:0+2*i], errFlag[i])/*synthesis syn_preserve = 1*/; 
		// end
	// endgenerate
	// // voters instantiation
	// genvar i;
	// generate
	// for (i=0;i<2; i=i+1)
		// begin: ripple
			// voter1 V(ctr[i],ctrL[0+i],ctrL[2+i],ctrL[4+i])/*synthesis syn_preserve = 1*/; 
		// end
	// endgenerate
// endmodule	
	
	