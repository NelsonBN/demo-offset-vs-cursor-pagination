db = db.getSiblingDB('DemoDB');

// Product names array for variety
const productNames = [
    'Motherboard', 'Keyboard', 'Mouse', 'Monitor', 'RAM', 'Hard Drive', 'Graphics Card',
    'Processor', 'Power Supply', 'Case', 'SSD', 'HDD', 'Optical Drive', 'Network Card',
    'Sound Card', 'Webcam', 'Headphones', 'Speakers', 'Microphone', 'USB Hub',
    'Router', 'Switch', 'Cable', 'Adapter', 'Battery', 'Charger', 'Fan', 'Cooler',
    'Thermal Paste', 'Screwdriver', 'Cable Tie', 'Anti-static Wrist Strap', 'Cleaning Kit',
    'USB Drive', 'Memory Card', 'Card Reader', 'External HDD', 'External SSD', 'Docking Station',
    'KVM Switch', 'UPS', 'Surge Protector', 'HDMI Cable', 'DisplayPort Cable', 'USB-C Cable',
    'Ethernet Cable', 'Wifi Adapter', 'Bluetooth Adapter', 'Gaming Mouse', 'Gaming Keyboard',
    'Gaming Headset', 'Gaming Chair', 'Monitor Stand', 'Laptop Stand', 'Tablet Stand',
    'Phone Stand', 'Desk Pad', 'Mouse Pad', 'Wrist Rest', 'Cable Management', 'RGB Strip',
    'LED Strip', 'Smart Bulb', 'Smart Switch', 'Smart Plug', 'Smart Speaker', 'Smart Display',
    'Action Camera', 'Drone', 'VR Headset', 'AR Glasses', 'Smart Watch', 'Fitness Tracker',
    'Portable Charger', 'Wireless Charger', 'Car Charger', 'Wall Mount', 'Ceiling Mount',
    'Security Camera', 'Door Bell', 'Smart Lock', 'Motion Sensor', 'Temperature Sensor',
    'Humidity Sensor', 'Air Quality Monitor', 'Smart Thermostat', 'Smart Fan', 'Smart Light',
    'Home Hub', 'Voice Assistant', 'Streaming Device', 'Media Player', 'Game Console',
    'Gaming Controller', 'Racing Wheel', 'Flight Stick', 'Drawing Tablet', 'Stylus',
    'Printer', 'Scanner', 'All-in-One Printer', 'Ink Cartridge', 'Toner Cartridge', 'Paper',
    'Label Maker', 'Laminator', 'Shredder', 'Projector', 'Projection Screen', 'Whiteboard',
    'Server', 'Workstation', 'Laptop', 'Desktop', 'Tablet', 'Smartphone', 'Smart TV',
    'Soundbar', 'Subwoofer', 'Amplifier', 'Mixer', 'DJ Controller', 'MIDI Keyboard',
    'Audio Interface', 'Studio Monitor', 'Condenser Mic', 'Dynamic Mic', 'Pop Filter',
    'Boom Arm', 'Shock Mount', 'Acoustic Panel', 'Bass Trap', 'Reflection Filter',
    'Light Ring', 'Softbox', 'Camera Tripod', 'Gimbal', 'Lens Filter', 'Memory Foam',
    'Ergonomic Cushion', 'Standing Desk', 'Desk Organizer', 'Monitor Arm', 'Cable Tray',
    'Power Strip', 'Extension Cord', 'Voltage Regulator', 'Inverter', 'Solar Panel',
    'Wind Turbine', 'Generator', 'Fuel Cell', 'Transformer', 'Circuit Breaker',
    'Multimeter', 'Oscilloscope', 'Signal Generator', 'Power Meter', 'Logic Analyzer',
    'Soldering Iron', 'Heat Gun', 'Wire Stripper', 'Crimping Tool', 'Breadboard',
    'Resistor', 'Capacitor', 'Inductor', 'Transistor', 'Diode', 'LED', 'Relay',
    'Connector', 'Terminal Block', 'PCB', 'Enclosure', 'Heat Sink', 'Thermal Pad',
    'Conductive Paste', 'Flux', 'Solder', 'Desoldering Braid', 'PCB Cleaner',
    'Compressed Air', 'Isopropyl Alcohol', 'Cotton Swab', 'Lint Free Cloth', 'Dust Cover',
    'Tool Kit', 'Precision Screwdriver', 'Hex Key Set', 'Pliers', 'Wire Cutters',
    'Digital Caliper', 'Magnifying Glass', 'ESD Mat', 'Ground Strap', 'Component Tester'
];

const suffixes = ['Pro', 'Ultra', 'Max', 'Plus', 'Elite', 'Premium', 'Standard', 'Basic'];
const sizes = ['', '', '', '', ' XL', ' L', ' M', ' S'];

// Helper function to generate UUID v4
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Clear existing products
db.Product.deleteMany({});

print('Starting bulk insert of 10,000,000 products...');

// MongoDB performs best with batch sizes between 1,000 and 10,000
const batchSize = 10000;
const totalRecords = 10000000;
const totalBatches = totalRecords / batchSize;

let insertedCount = 0;

for (let batch = 0; batch < totalBatches; batch++) {
    const products = [];

    for (let i = 0; i < batchSize; i++) {
        const index = batch * batchSize + i;
        const productName = productNames[index % 150];

        let suffix;
        if (index % 20 === 0) suffix = 'Pro';
        else if (index % 17 === 0) suffix = 'Ultra';
        else if (index % 13 === 0) suffix = 'Max';
        else if (index % 11 === 0) suffix = 'Plus';
        else if (index % 7 === 0) suffix = 'Elite';
        else if (index % 5 === 0) suffix = 'Premium';
        else if (index % 3 === 0) suffix = 'Standard';
        else suffix = 'Basic';

        let size = '';
        if (index % 100000 === 0) size = ' XL';
        else if (index % 50000 === 0) size = ' L';
        else if (index % 25000 === 0) size = ' M';
        else if (index % 10000 === 0) size = ' S';

        const version = (index % 10) + 1;
        const name = `${productName} ${suffix}${size} v${version}`;
        const quantity = Math.floor(Math.random() * 100) + 1;

        products.push({
            _id: generateUUID(),
            Name: name,
            Quantity: quantity
        });
    }

    // Insert batch with ordered:false for better performance
    db.Product.insertMany(products, { ordered: false });

    insertedCount += batchSize;

    // Progress update every 100 batches (1,000,000 records)
    if ((batch + 1) % 100 === 0) {
        print(`Progress: ${insertedCount.toLocaleString()} / ${totalRecords.toLocaleString()} records inserted (${((insertedCount / totalRecords) * 100).toFixed(1)}%)`);
    }
}

print(`\nCompleted! Total products inserted: ${insertedCount.toLocaleString()}`);

// Create index on _id (already exists by default, but explicit for clarity)
// print('Ensuring indexes...');
// db.Product.createIndex({ _id: 1 });
// db.Product.createIndex({ Name: 1 });
// db.Product.createIndex({ Quantity: 1 });
//print('Indexes created successfully!');

print('Collection stats:');
printjson(db.Product.stats());
