import pandas as pd
from statsmodels.stats.anova import AnovaRM
import matplotlib.pylab as plt

data = pd.read_csv("rankings.csv")

data = data[data["machine"] == 1]

data_long = pd.melt(data, 
                    id_vars=['participant', 'machine'], 
                    value_vars=['phoneRank', 'staticRank', 'AdaptiveRank'],
                    var_name='method', 
                    value_name='rank')

anova = AnovaRM(data_long, depvar='rank', subject='participant', within=['method'])
result = anova.fit()
print(result)

plt.figure(figsize=(10, 6))
plt.scatter(data=data_long, x='participant', y='rank')
plt.title("Participant Rankings by Method (Machine 1)")
plt.xlabel("Participant")
plt.ylabel("Rank")
plt.legend(title="Method")
plt.grid(True)
plt.tight_layout()
plt.show()


''' TREADMILL
               Anova
====================================
       F Value Num DF  Den DF Pr > F
------------------------------------
method 10.1282 2.0000 30.0000 0.0004
====================================
'''

''' ELLIPTICAL
               Anova
====================================
       F Value Num DF  Den DF Pr > F
------------------------------------
method  7.3256 2.0000 30.0000 0.0026
====================================
'''

''' ROW
               Anova
====================================
       F Value Num DF  Den DF Pr > F
------------------------------------
method 14.2987 2.0000 30.0000 0.0000
====================================
'''

