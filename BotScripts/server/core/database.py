import sqlite3
from common.interfaces.dbInterface import DBInterface


class Database(DBInterface):
    def __init__(self, dbUrl: str = ':memory:'):
        self.connection = sqlite3.connect(dbUrl, check_same_thread=False)
        self.cursor = self.connection.cursor()
    

    def connect(self):
        self.connection = sqlite3.connect(':memory:')
        self.cursor = self.connection.cursor()


    def query(self, sql: str, params=None):
        if params:
            self.cursor.execute(sql, params)
        else:
            self.cursor.execute(sql)
        
        self.connection.commit()
        return self.cursor.fetchall()
