//  Detector of 2 zeros and 2 ones in 4-bits word
module voter1(out, a, b, c, d); // detector
	output  out;
	input a, b, c, d;
	//wire eq;
	assign v1=(a== b)?a:c;
	assign v2=(b==c)?b:d;
	assign v3=(c==d)?c:a;
	
	assign c1=(v1==v2);
	assign c2=(v2==v3);
	assign out = (c1 && c2);
endmodule