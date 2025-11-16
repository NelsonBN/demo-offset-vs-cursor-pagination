-- Generate 10,000,000 product records using PostgreSQL's generate_series
-- This approach is memory efficient and fast for PostgreSQL

-- First, create a temporary table with diverse product names to avoid repetition
CREATE TEMPORARY TABLE product_names AS
SELECT unnest(ARRAY[
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
]) AS name;

-- Clear existing data and reset the identity sequence
TRUNCATE TABLE Product RESTART IDENTITY;

-- Insert 10,000,000 records using generate_series for efficiency
-- Using modulo to cycle through product names to ensure variety
INSERT INTO Product (Name, Quantity, Code)
SELECT
    (SELECT name FROM product_names OFFSET (i % 150) LIMIT 1) || ' ' ||
    CASE
        WHEN i % 20 = 0 THEN 'Pro'
        WHEN i % 17 = 0 THEN 'Ultra'
        WHEN i % 13 = 0 THEN 'Max'
        WHEN i % 11 = 0 THEN 'Plus'
        WHEN i % 7 = 0 THEN 'Elite'
        WHEN i % 5 = 0 THEN 'Premium'
        WHEN i % 3 = 0 THEN 'Standard'
        ELSE 'Basic'
    END ||
    CASE
        WHEN i % 100000 = 0 THEN ' XL'
        WHEN i % 50000 = 0 THEN ' L'
        WHEN i % 25000 = 0 THEN ' M'
        WHEN i % 10000 = 0 THEN ' S'
        ELSE ''
    END ||
    ' v' || ((i % 10) + 1)::text AS Name,
    (RANDOM() * 100 + 1)::INTEGER AS Quantity,
    gen_random_uuid() AS Code
FROM generate_series(1, 10000000) AS i;

-- Clean up temporary table
DROP TABLE product_names;
