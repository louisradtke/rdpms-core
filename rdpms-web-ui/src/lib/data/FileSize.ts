export class FileSize {
    private bytes: number;

    constructor(bytes: number) {
        this.bytes = bytes;
    }

    getString(format: string, showUnit: boolean = true, decimalPlaces?: number): string {
        // Parse the format string
        const isBits = format.includes('b'); // lowercase 'b' for bits
        const isBytes = format.includes('B'); // uppercase 'B' for bytes
        const isBinary = format.includes('i'); // 'i' indicates binary (base 1024)
        const isAuto = format.includes('*'); // '*' indicates auto-selection

        if (!(!isBits && isBytes || isBits && !isBytes)) {
            throw new Error('Format must contain either "b" or "B" for bits or bytes, but not both.');
        }

        // Determine the base
        const base = isBinary ? 1024 : 1000;
        
        // Handle auto-selection of unit
        if (isAuto) {
            const unitPrefixes = ['', 'k', 'm', 'g', 't', 'p', 'e'];
            let bestUnit = '';
            let bestValue = isBits ? this.bytes * 8 : this.bytes;
            
            // Find the largest unit where the value remains > 1
            for (let i = 1; i < unitPrefixes.length; i++) {
                const divisor = Math.pow(base, i);
                let testValue = this.bytes / divisor;
                
                if (isBits) {
                    testValue *= 8;
                }
                
                if (testValue >= 1) {
                    bestUnit = unitPrefixes[i];
                    bestValue = testValue;
                } else {
                    break;
                }
            }

            if (bestUnit === '') {
                decimalPlaces = 0;
            }

            // Format the number with specified or automatic precision
            let formattedValue: string;
            if (decimalPlaces !== undefined) {
                formattedValue = bestValue.toFixed(decimalPlaces);
            } else {
                // Auto precision based on value magnitude
                formattedValue = bestValue < 10 ? bestValue.toFixed(2) : 
                                bestValue < 100 ? bestValue.toFixed(1) : 
                                Math.round(bestValue).toString();
            }

            // Return just the number if showUnit is false
            if (!showUnit) {
                return formattedValue;
            }

            // Build the unit string
            let unitString = bestUnit.toUpperCase();
            if (isBinary && bestUnit) {
                unitString += 'i';
            }
            unitString += isBits ? 'b' : 'B';

            return `${formattedValue} ${unitString}`;
        }
        
        // Extract the unit prefix (K, M, G, etc.)
        const unitMatch = format.match(/([KMGTPE])/i);
        if (!unitMatch) {
            // No prefix, return raw bytes or bits
            let value = isBits ? this.bytes * 8 : this.bytes;
            
            if (decimalPlaces !== undefined) {
                value = Number(value.toFixed(decimalPlaces));
            }
            
            if (!showUnit) {
                return value.toString();
            }
            
            return `${value} ${isBits ? 'b' : 'B'}`;
        }

        const unitPrefix = unitMatch[1].toLowerCase();
        
        // Define the multipliers for each unit
        const multipliers: { [key: string]: number } = {
            'k': 1,
            'm': 2,
            'g': 3,
            't': 4,
            'p': 5,
            'e': 6
        };

        const power = multipliers[unitPrefix];
        if (power === undefined) {
            throw new Error(`Unsupported unit prefix: ${unitPrefix}`);
        }

        // Calculate the divisor
        const divisor = Math.pow(base, power);
        
        // Convert bytes to the requested unit
        let value = this.bytes / divisor;
        
        // Convert to bits if requested
        if (isBits) {
            value *= 8;
        }

        // Format the number with specified or automatic precision
        let formattedValue: string;
        if (decimalPlaces !== undefined) {
            formattedValue = value.toFixed(decimalPlaces);
        } else {
            // Auto precision based on value magnitude
            formattedValue = value < 10 ? value.toFixed(2) : 
                            value < 100 ? value.toFixed(1) : 
                            Math.round(value).toString();
        }

        // Return just the number if showUnit is false
        if (!showUnit) {
            return formattedValue;
        }

        // Build the unit string
        let unitString = unitPrefix.toUpperCase();
        if (isBinary) {
            unitString += 'i';
        }
        unitString += isBits ? 'b' : 'B';

        return `${formattedValue} ${unitString}`;
    }

    getBytes(): number {
        return this.bytes;
    }

    setBytes(bytes: number): void {
        this.bytes = bytes;
    }
}