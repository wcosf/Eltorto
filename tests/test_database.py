#проверка дампа
def test_database_dump_loaded_successfully(db_connection):
    cursor = db_connection.cursor()

    cursor.execute("""
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_name IN ('Cakes', 'Categories', 'Fillings', 'Orders')
    """)
    existing_tables = [row[0] for row in cursor.fetchall()]
    
    for table in ['Cakes', 'Categories', 'Fillings', 'Orders']:
        assert table in existing_tables, f"Table {table} does not exist"

    cursor.execute('SELECT COUNT(*) FROM "Cakes"')
    assert cursor.fetchone()[0] > 0, "No cakes found in database"
    
    cursor.execute('SELECT COUNT(*) FROM "Categories"')
    assert cursor.fetchone()[0] > 0, "No categories found"
    
    cursor.execute('SELECT COUNT(*) FROM "Fillings"')
    assert cursor.fetchone()[0] > 0, "No fillings found"

#структура таблицы
def test_cakes_have_correct_columns(db_connection):
    cursor = db_connection.cursor()
    cursor.execute("""
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'Cakes'
    """)
    columns = [row[0] for row in cursor.fetchall()]
    
    expected_columns = ['Id', 'Name', 'ImageUrl', 'ThumbnailUrl', 'CategorySlug', 'IsFeatured', 'Description']
    for col in expected_columns:
        assert col in columns, f"Column {col} not found in Cakes table"