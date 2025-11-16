db = db.getSiblingDB('DemoDB');

db.createCollection('Product');

db.Product.insertMany([
    { _id: '454aee83-f484-40d7-8ca8-f272b128ebc4', Name: 'Motherboard',   Quantity: 23 },
    { _id: 'c0b0b6e0-5b0e-4b0e-8b0e-0b0e0b0e0b0e', Name: 'Keyboard',      Quantity: 4  },
    { _id: '9dd437d3-a1be-44dc-9d63-d6547350bf1d', Name: 'Mouse',         Quantity: 7  },
    { _id: '597a6b04-d1fe-4c8b-a371-8a8cda4eac71', Name: 'Monitor',       Quantity: 15 },
    { _id: 'b0b0b6e0-5b0e-4b0e-8b0e-0b0e0b0e0b0e', Name: 'RAM',           Quantity: 50 },
    { _id: '2e580764-3f9e-4580-b601-661cf845fb68', Name: 'Hard Drive',    Quantity: 32 },
    { _id: '0bb85bcb-a33a-4332-983e-a87ff796c829', Name: 'Graphics Card', Quantity: 10 },
    { _id: 'bcc0214f-37a5-48af-8eea-9dffe12d40b9', Name: 'Processor',     Quantity: 12 },
    { _id: 'c54792a5-db35-4c5e-ba79-a195d3ce4c44', Name: 'Power Supply',  Quantity: 20 },
    { _id: '113a4d79-ad9e-4933-90b4-aa5a48ff7ce3', Name: 'Case',          Quantity: 30 },
    { _id: 'f8a9d2c3-4b5e-6789-abcd-ef0123456789', Name: 'Coffee Mug',    Quantity: 25 },
    { _id: '12345678-90ab-cdef-1234-567890abcdef', Name: 'Notebook',      Quantity: 30 },
    { _id: 'abcdef12-3456-7890-abcd-ef1234567890', Name: 'Umbrella',      Quantity: 15 },
    { _id: '98765432-10ab-cdef-9876-543210abcdef', Name: 'Backpack',      Quantity: 20 },
    { _id: 'fedcba09-8765-4321-fedc-ba0987654321', Name: 'Sunglasses',    Quantity: 25 },
    { _id: '11223344-5566-7788-9900-aabbccddeeff', Name: 'Water Bottle',  Quantity: 15 },
    { _id: 'aabbccdd-eeff-1122-3344-556677889900', Name: 'Bicycle',       Quantity: 10 },
    { _id: '99887766-5544-3322-1100-ffeeddccbbaa', Name: 'Sneakers',      Quantity: 25 },
    { _id: '13579bdf-2468-ace0-1357-9bdf2468ace0', Name: 'Watch',         Quantity: 20 },
    { _id: '2468ace0-1357-9bdf-2468-ace013579bdf', Name: 'Scarf',         Quantity: 30 },
    { _id: 'ace01357-9bdf-2468-ace0-13579bdf2468', Name: 'Cookbook',      Quantity: 15 },
    { _id: '9bdf2468-ace0-1357-9bdf-2468ace01357', Name: 'Pillow',        Quantity: 10 },
    { _id: 'bdf2468a-ce01-3579-bdf2-468ace013579', Name: 'Guitar',        Quantity: 12 },
    { _id: 'f2468ace-0135-79bd-f246-8ace013579bd', Name: 'Paintbrush',    Quantity: 20 },
    { _id: '468ace01-3579-bdf2-468a-ce013579bdf2', Name: 'Candle',        Quantity: 50 },
    { _id: '8ace0135-79bd-f246-8ace-013579bdf246', Name: 'Teapot',        Quantity: 12 },
    { _id: 'ce013579-bdf2-468a-ce01-3579bdf2468a', Name: 'Blanket',       Quantity: 30 },
    { _id: '013579bd-f246-8ace-0135-79bdf2468ace', Name: 'Journal',       Quantity: 50 },
    { _id: '3579bdf2-468a-ce01-3579-bdf2468ace01', Name: 'Lamp',          Quantity: 15 },
    { _id: '79bdf246-8ace-0135-79bd-f2468ace0135', Name: 'Flower Vase',   Quantity: 12 }
]);
